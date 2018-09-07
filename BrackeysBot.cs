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

using Newtonsoft.Json;

using BrackeysBot.Data;
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
        private CooldownData _cooldowns;

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
            _cooldowns = JsonConvert.DeserializeObject<CooldownData> (File.ReadAllText ("cooldowns.json"));

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
            CommandInfo executedCommand = _commandService.Search (context, argPos).Commands [0].Command;

            bool cooldownCommand = CheckIfCommandHasCooldown (executedCommand.Name.ToLower ());

            if (cooldownCommand && !UserHelper.HasStaffRole (s.Author as IGuildUser))
            {
                bool sameParamCommand = CheckIfSameParameterCommand (executedCommand.Name.ToLower ());

                string parameters = s.ToString ().Remove (0, s.ToString ().IndexOf (' ') + 1);

                bool cooldownExpired = HasCooldownExpired (executedCommand.Name, s.Author as IGuildUser, sameParamCommand, parameters);

                if (cooldownExpired)
                {
                    AddUserToCooldown (executedCommand.Name, s.Author as IGuildUser, sameParamCommand, parameters);
                }
                else
                {
                    TimeSpan ts = GetTimeUntilCooldownHasExpired (executedCommand.Name.ToLower (), s.Author as IGuildUser, sameParamCommand, parameters);

                    if (executedCommand.Name.ToLower () == "thanks")
                    {
                        Embed eb = new EmbedBuilder ()
                            .WithTitle ("You can't thank that user yet")
                            .WithDescription ($"{s.Author.Mention}, you can't thank that user yet. Please wait {ts.Hours} hours, {ts.Minutes} minutes and {ts.Seconds} seconds.")
                            .WithColor (Color.Orange);

                        await context.Channel.SendMessageAsync (string.Empty, false, eb);
                        return;
                    }
                    else
                    {
                        Embed eb = new EmbedBuilder ()
                            .WithTitle ("Cooldown hasn't expired yet")
                            .WithDescription ($"{s.Author.Mention}, you can't run this command yet. Please wait {ts.Hours} hours, {ts.Minutes} minutes and {ts.Seconds} seconds.")
                            .WithColor (Color.Orange);

                        await context.Channel.SendMessageAsync (string.Empty, false, eb);
                        return;
                    }
                }
            }

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

        private bool CheckIfCommandHasCooldown (string commandName)
        {
            if (_cooldowns.Commands.Any (c => c.CommandName == commandName)) return true;
            if (_cooldowns.SameParameterCommands.Any (c => c.CommandName == commandName)) return true;
            return false;
        }

        private bool CheckIfSameParameterCommand (string commandName)
        {
            if (_cooldowns.Commands.Any (c => c.CommandName == commandName)) return false;
            if (_cooldowns.SameParameterCommands.Any (c => c.CommandName == commandName)) return true;
            throw new ArgumentException ("Command isn't a normal command nor a same parameter command.");
        }

        private TimeSpan GetTimeUntilCooldownHasExpired (string commandName, IGuildUser user, bool sameParameters, string parameters = "")
        {
            if (sameParameters && !string.IsNullOrEmpty (parameters))
            {
                CommandCooldown<UserCooldownParameters> sameParamCool = _cooldowns.SameParameterCommands.FirstOrDefault (c => c.CommandName == commandName);
                UserCooldownParameters usrCool = sameParamCool.Users.FirstOrDefault (u => u.Id == user.Id && u.Parameters == parameters);

                DateTime executedTime = usrCool.CommandExecutedTime.ToDateTime ();
                DateTime currentTime = DateTime.UtcNow;

                return executedTime.AddSeconds (sameParamCool.CooldownTime) - currentTime;                 
            }
            else
            {
                CommandCooldown<UserCooldown> cmdCool = _cooldowns.Commands.FirstOrDefault (c => c.CommandName == commandName);
                UserCooldown usrCool = cmdCool.Users.FirstOrDefault (u => u.Id == user.Id);

                DateTime executedTime = usrCool.CommandExecutedTime.ToDateTime ();
                DateTime currentTime = DateTime.UtcNow;

                return executedTime.AddSeconds (cmdCool.CooldownTime) - currentTime;    
            }
        }

        private T GetUserCooldown<T> (string commandName, IGuildUser user, bool sameParameters = false, string parameters = "") where T : UserCooldown
        {
            if (sameParameters && !string.IsNullOrEmpty (parameters))
            {
                CommandCooldown<UserCooldownParameters> sameParamCool = _cooldowns.SameParameterCommands.FirstOrDefault (c => c.CommandName == commandName);
                
                // If the command couldn't be found in same parameter cooldowns list then the command has no cooldown options
                if (sameParamCool == null) return null;

                UserCooldownParameters usrCool = sameParamCool.Users.FirstOrDefault (u => u.Id == user.Id && u.Parameters == parameters);

                // This user isn't listed yet, meaning the cooldown has either expired or the user hasn't ran the command yet
                if (usrCool == null) return null;

                return (T) ((UserCooldown) usrCool);
            }
            else if (sameParameters && string.IsNullOrEmpty (parameters))
            {
                throw new ArgumentException ("Parameters are empty, and checking for same parameter cooldowns.");
            }
            else
            {
                CommandCooldown<UserCooldown> cmdCool = _cooldowns.Commands.FirstOrDefault (c => c.CommandName == commandName);

                // If the command couldn't be found, then search in the same parameter cooldowns list
                if (cmdCool == null) return null;

                UserCooldown usrCool = cmdCool.Users.FirstOrDefault (u => u.Id == user.Id);

                // This user isn't listed yet, meaning the cooldown has either expired or the user hasn't ran the command yet.
                if (usrCool == null) return null;

                return (T) usrCool;
            }
        }

        /// <summary>
        /// Checks if the cooldown expired.
        /// </summary>
        private bool HasCooldownExpired (string commandName, IGuildUser user, bool sameParameters = false, string parameters = "")
        {
            if (GetUserCooldown<UserCooldown> (commandName, user, sameParameters, parameters) == null) return true;
            if (sameParameters)
                if (GetUserCooldown<UserCooldownParameters> (commandName, user, sameParameters, parameters) == null) return true;

            TimeSpan ts = GetTimeUntilCooldownHasExpired (commandName, user, sameParameters, parameters);
            if (ts.Ticks < 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Adds a user to the cooldown list. If he already exists, the user's executed time is just changed.
        /// </summary>
        private void AddUserToCooldown (string commandName, IGuildUser user, bool sameParameters = false, string parameters = "")
        {
            if (sameParameters && !string.IsNullOrEmpty (parameters))
            {
                CommandCooldown<UserCooldownParameters> cmdCool = _cooldowns.SameParameterCommands.FirstOrDefault (c => c.CommandName == commandName);
                UserCooldownParameters usrCool = GetUserCooldown<UserCooldownParameters> (commandName, user, true, parameters);
                if (usrCool == null)
                    cmdCool.Users.Add (new UserCooldownParameters { Id = user.Id, CommandExecutedTime = DateTime.UtcNow.ToTimestamp (), Parameters = parameters });
                else
                    usrCool.CommandExecutedTime = DateTime.UtcNow.ToTimestamp ();
            }
            else
            {
                CommandCooldown<UserCooldown> cmdCool = _cooldowns.Commands.FirstOrDefault (c => c.CommandName == commandName);
                UserCooldown usrCool = GetUserCooldown<UserCooldown> (commandName, user, false, "");
                if (usrCool == null)
                    cmdCool.Users.Add (new UserCooldown { Id = user.Id, CommandExecutedTime = DateTime.UtcNow.ToTimestamp () });
                else
                    usrCool.CommandExecutedTime = DateTime.UtcNow.ToTimestamp ();
            }            

            SaveCooldowns ();
        }

        private void SaveCooldowns ()
            => File.WriteAllText ("cooldowns.json", JsonConvert.SerializeObject (_cooldowns, Formatting.Indented));

        /// <summary>
        /// Registers a method to handle massive codeblocks.
        /// </summary>
        private void RegisterMassiveCodeblockHandle ()
        {
            _client.MessageReceived += HandleMassiveCodeblock;
        }
        /// <summary>
        /// Handles a massive codeblock.
        /// </summary>
        private async Task HandleMassiveCodeblock (SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            // Ignore specific channels
            ulong[] ignoreChannelIds = _settings["massivecodeblock-ignore"].Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
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
