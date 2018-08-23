using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using BrackeysBot.Commands;

namespace BrackeysBot 
{
    public sealed class BrackeysBot 
    {
        public IConfiguration Configuration { get; set; }

        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commandService;

        private KarmaTable _karma;
        private SettingsTable _settings;
        private RuleTable _rules;
        private UnityDocs _unityDocs;

        private readonly Regex _jobRegex = new Regex(@"(```.*\[Hiring\]\n.*\n.*Name:.*\n.*Required:.*\n.*Portfolio.*\nTeam Size:.*\n.*Length.*\nCompensation:.*\nResponsibilities:.*\n.*Description:.*```)|(```.*\[Looking for work\]\n.*\n.*Role:.*\nSkills:.*\n.*Portfolio.*\nExperience.*\nRates:.*```)|(```.*\[Hiring\]\n.*\n.*Name:.*\n.*Required:.*\n.*Portfolio.*\n.*Description:.*```)|(```.*\[Looking for work\]\n.*\n.*Role:.*\nSkills:.*\n.*Portfolio.*```)|(```.*\[Recruiting\]\n--------------------------------\n.*Name:.*\nProject Description:.*```)|(```.*\[Looking to mentor\]\n.*\n.*interest:.*\nRates.*```)|(```.*\[Looking for a mentor\]\n.*\n.*interest:.*\nRates.*```)".ToLower(), RegexOptions.Compiled | RegexOptions.Singleline);

        private Commands.LeaderboardCommand.LeaderboardNavigator _leaderboardNavigator;

        public BrackeysBot ()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public async Task Start () 
        {
            _client = new DiscordSocketClient();

            _commandService = new CommandService();

            _karma = new KarmaTable("karma.json");
            _settings = new SettingsTable("settings.json");

            _rules = new RuleTable("rules.json");
            _unityDocs = new UnityDocs ("manualReference.json", "scriptReference.json");

            _leaderboardNavigator = new Commands.LeaderboardCommand.LeaderboardNavigator(_karma, _settings);

            _services = new ServiceCollection()

                // Add the command service
                .AddSingleton(_commandService)

                .AddSingleton(Configuration)

                // Add the singletons for the databases
                .AddSingleton(_karma)
                .AddSingleton(_settings)
                .AddSingleton(_rules)
                .AddSingleton(_unityDocs)

                .AddSingleton(_leaderboardNavigator)

                // Finally, build the provider
                .BuildServiceProvider();

            await InstallCommands();

            RegisterMassiveCodeblockHandle();
            RegisterLeaderboardNavigationHandle();

            await _client.LoginAsync(TokenType.Bot, Configuration["token"]);
            await _client.SetGameAsync($"{ Configuration["prefix"] }help");
            await _client.StartAsync();
        }

        /// <summary>
        /// Installs the command handling to the client.
        /// </summary>
        private async Task InstallCommands ()
        {
            _client.MessageReceived += HandleCommand;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Handles a command, represented in the given message.
        /// </summary>
        private async Task HandleCommand(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            int argPos = 0;
            
            if (!msg.HasStringPrefix(Configuration["prefix"], ref argPos)
                && !msg.Content.ToLower().StartsWith("thanks")) return;

            CommandContext context = new CommandContext(_client, msg);
            
            IResult result = await _commandService.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand
                    || result.Error == CommandError.BadArgCount)
                {
                    return;
                }

                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription(result.ErrorReason)
                    .WithColor(Color.Red);

                await context.Channel.SendMessageAsync(string.Empty, false, builder);
            }
        }

        /// <summary>
        /// Registers a method to handle massive codeblocks.
        /// </summary>
        private void RegisterMassiveCodeblockHandle ()
        {
            _client.MessageReceived += HandleMassiveCodeblock;
            _client.MessageReceived += CheckTemplate;
        }

        public async Task CheckTemplate (SocketMessage s)
        {
            ulong[] ignoreChannelIds = _settings["job-channel-ids"].Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
            if (ignoreChannelIds.All(id => id != s.Channel.Id)) return;
            if (!_jobRegex.IsMatch(s.Content.ToLower()))
            {
                if (!(s.Author as IGuildUser).HasStaffRole(_settings))
                {
                    if (!s.Author.IsBot)
                        await s.Author.SendMessageAsync($"Hi, {s.Author.Username}. I've removed the message you sent in #{s.Channel.Name} at {s.Timestamp.DateTime.ToString()} UTC, because you didn't follow the template. Please re-post it using the provided template that is pinned to that channel.");
                    await s.DeleteAsync();
                }
            }
        }
        /// <summary>
        /// Handles a massive codeblock.
        /// </summary>
        private async Task HandleMassiveCodeblock (SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            // Ignore specific channels
            ulong[] ignoreChannelIds = _settings["job-channel-ids"].Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
            if (ignoreChannelIds.Any(id => id == s.Channel.Id)) return;

            await Commands.HasteCommand.HasteIfMassiveCodeblock(s);
        }
        /// <summary>
        /// Handles a leaderboard navigation event.
        /// </summary>
        private void RegisterLeaderboardNavigationHandle()
        {
            _client.ReactionAdded += _leaderboardNavigator.HandleLeaderboardNavigation;
        }
    }
}