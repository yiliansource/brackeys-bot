using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        public IConfiguration Configuration { get; set; }

        public DataModule Data { get; set; }
        public CommandHandler Commands { get; set; }

        private IServiceProvider _services;
        private DiscordSocketClient _client;

        private EventPointCommand.LeaderboardNavigator _leaderboardNavigator;

        /// <summary>
        /// Creates a new instance of the bot and initializes the configuration.
        /// </summary>
        public BrackeysBot ()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        /// <summary>
        /// Starts the execution of the bot.
        /// </summary>
        public async Task Start () 
        {
            _client = new DiscordSocketClient();

            Data = new DataModule();
            Data.InitializeDataFiles();

            Commands = new CommandHandler(Data, Configuration["prefix"]);

            _leaderboardNavigator = new EventPointCommand.LeaderboardNavigator(Data.EventPoints, Data.Settings);

            _services = new ServiceCollection()

                // Add the command service
                .AddSingleton(Commands.Service)

                // Add the configuration
                .AddSingleton(Configuration)

                // Add the singletons for the databases
                .AddSingleton(Data.EventPoints)
                .AddSingleton(Data.Settings)
                .AddSingleton(Data.Statistics)
                .AddSingleton(Data.CustomCommands)
                .AddSingleton(Data.Cooldowns)
                .AddSingleton(Data.Rules)
                .AddSingleton(Data.UnityDocs)

                .AddSingleton(_leaderboardNavigator)

                // Finally, build the provider
                .BuildServiceProvider();

            Commands.ServiceProvider = _services;
            await Commands.InstallCommands(_client);

            UserHelper.Settings = Data.Settings;

            RegisterMassiveCodeblockHandle();
            RegisterLeaderboardNavigationHandle();

            await _client.LoginAsync(TokenType.Bot, Configuration["token"]);
            await _client.SetGameAsync($"{ Configuration["prefix"] }help");
            await _client.StartAsync();
        }
        
        
        /// <summary>
        /// Registers a method to handle massive codeblocks.
        /// </summary>
        private void RegisterMassiveCodeblockHandle ()
        {
            _client.MessageReceived += HandleMassiveCodeblock;
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
        private async Task HandleMassiveCodeblock (SocketMessage s)
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
