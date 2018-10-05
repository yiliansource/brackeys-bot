using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Commands;
using BrackeysBot.Modules;

namespace BrackeysBot
{
    public sealed class BrackeysBot
    {
        public static IConfiguration Configuration { get; set; }

        public DataModule Data { get; set; }
        public CommandHandler Commands { get; set; }

        private IServiceProvider _services;
        private DiscordSocketClient _client;

        private EventPointCommand.LeaderboardNavigator _leaderboardNavigator;

        /// <summary>
        /// Creates a new instance of the bot and initializes the configuration.
        /// </summary>
        public BrackeysBot()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        /// <summary>
        /// Starts the execution of the bot.
        /// </summary>
        public async Task Start()
        {
            _client = new DiscordSocketClient();

            Data = new DataModule();
            Data.InitializeDataFiles();

            Commands = new CommandHandler(Data, Configuration["prefix"]);

            _leaderboardNavigator = new EventPointCommand.LeaderboardNavigator(Data.EventPoints, Data.Settings);

            _services = new ServiceCollection()

                // Add the command service
                .AddSingleton(Commands.Service)

                // Add the singletons for the databases
                .AddSingleton(Data.EventPoints)
                .AddSingleton(Data.Settings)
                .AddSingleton(Data.Statistics)
                .AddSingleton(Data.CustomCommands)
                .AddSingleton(Data.Cooldowns)
                .AddSingleton(Data.Rules)
                .AddSingleton(Data.UnityDocs)
                .AddSingleton(Data.Mutes)

                .AddSingleton(_leaderboardNavigator)

                // Finally, build the provider
                .BuildServiceProvider();

            Commands.ServiceProvider = _services;
            await Commands.InstallCommands(_client);

            UserHelper.Settings = Data.Settings;

            RegisterMuteOnJoin();
            RegisterMassiveCodeblockHandle();
            RegisterLeaderboardNavigationHandle();
            _ = PeriodicCheckMute(new TimeSpan(TimeSpan.TicksPerSecond * 5), System.Threading.CancellationToken.None);

            await _client.LoginAsync(TokenType.Bot, Configuration["token"]);
            await _client.SetGameAsync($"{ Configuration["prefix"] }help");
            await _client.StartAsync();
        }


        /// <summary>
        /// Registers a method to handle massive codeblocks.
        /// </summary>
        private void RegisterMassiveCodeblockHandle()
        {
            _client.MessageReceived += HandleMassiveCodeblock;
        }

        /// <summary>
        /// Registers a method to mute people who were muted but decided to be clever
        /// and wanted to rejoin to lose the muted role.
        /// </summary>
        private void RegisterMuteOnJoin()
        {
            _client.UserJoined += CheckMuteOnJoin;
        }

        async Task CheckMuteOnJoin(SocketGuildUser user)
        {
            if (DateTime.UtcNow.ToBinary() < long.Parse(Data.Mutes.GetOrDefault(user.Id.ToString() + "," + user.Guild.Id.ToString())))
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"));
            else
                await user.RemoveRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"));
        }

        public async Task PeriodicCheckMute(TimeSpan interval, System.Threading.CancellationToken cancellationToken)
        {
            while (true)
            {
                Parallel.For(0, Data.Mutes.Mutes.Count,
                   async index =>
                   {
                       try
                       {
                           var current = Data.Mutes.Mutes.ElementAt(index);
                           if (DateTime.UtcNow.ToBinary() >= long.Parse(current.Value))
                           {
                               SocketGuild guild = _client.GetGuild(ulong.Parse(current.Key.Split(',')[1]));
                               SocketGuildUser user = guild.GetUser(ulong.Parse(current.Key.Split(',')[0]));
                               await user.RemoveRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Name == "Muted"));
                           }
                       }
                       catch { }
                   });
                await Task.Delay(interval, cancellationToken);
            }
        }

        /// <summary>
        /// Registers the handle for a leaderboard navigation event.
        /// </summary>
        private void RegisterLeaderboardNavigationHandle()
        {
            _client.ReactionAdded += _leaderboardNavigator.HandleLeaderboardNavigation;
        }

        /// <summary>
        /// Handles a massive codeblock.
        /// </summary>
        private async Task HandleMassiveCodeblock(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            // Ignore specific channels
            if (Data.Settings.Has("job-channel-ids"))
            {
                ulong[] ignoreChannelIds = Data.Settings.Get("job-channel-ids").Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
                if (ignoreChannelIds.Any(id => id == s.Channel.Id)) return;
            }

            await PasteCommand.PasteIfMassiveCodeblock(s);
        }
    }
}
