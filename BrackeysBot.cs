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
using BrackeysBot.Listeners;

namespace BrackeysBot 
{
    public sealed class BrackeysBot 
    {
        public IConfiguration Configuration { get; set; }

        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commandService;

        private EventPointTable _eventPoints;
        private SettingsTable _settings;
        private StatisticsTable _statistics;
        private RuleTable _rules;
        private CustomizedCommandTable _customCommands;
        private UnityDocs _unityDocs;
        private CooldownData _cooldowns;
        
        private ArchiveListener _archiveListener;

        private EventPointCommand.LeaderboardNavigator _leaderboardNavigator;
      
        private static readonly string[] templateFiles = { "template-appsettings.json", "template-cooldowns.json", "template-rules.json", "template-settings.json" };

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

            _commandService = new CommandService();

            EnsurePreDataFiles();

            _eventPoints = new EventPointTable("eventPoints.json");
            _settings = new SettingsTable("settings.json");
            _statistics = new StatisticsTable("statistics.json");
            _customCommands = new CustomizedCommandTable("custom-commands.json");
            _rules = new RuleTable("rules.json");
            _unityDocs = new UnityDocs ("manualReference.json", "scriptReference.json");
            _cooldowns = CooldownData.FromPath("cooldowns.json");
            
            _archiveListener = new ArchiveListener();

            _leaderboardNavigator = new EventPointCommand.LeaderboardNavigator(_eventPoints, _settings);

            _services = new ServiceCollection()

                // Add the command service
                .AddSingleton(_commandService)

                // Add the configuration
                .AddSingleton(Configuration)

                // Add the singletons for the databases
                .AddSingleton(_eventPoints)
                .AddSingleton(_settings)
                .AddSingleton(_statistics)
                .AddSingleton(_customCommands)
                .AddSingleton(_cooldowns)
                .AddSingleton(_rules)
                .AddSingleton(_unityDocs)
                
                .AddSingleton(_archiveListener)

                .AddSingleton(_leaderboardNavigator)

                // Finally, build the provider
                .BuildServiceProvider();

            await InstallCommands();
            UserHelper._settings = _settings;

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
                && !msg.Content.ToLower().StartsWith("thank")) return;

            CommandContext context = new CommandContext(_client, msg);
            CommandInfo executedCommand = null;
            try
            {
                executedCommand = _commandService.Search(context, argPos).Commands[0].Command;
            }
            catch
            {
                // The executed command wasnt found in the modules, therefore look if any custom commands are registered.
                string command = msg.Content.Substring(argPos);
                if (_customCommands.Has(command))
                {
                    string message = _customCommands.Get(command);
                    await context.Channel.SendMessageAsync(message);
                }
                return;
            }

            if (executedCommand.Attributes.FirstOrDefault(a => a is HelpDataAttribute) is HelpDataAttribute data)
            {
                Embed eb = new EmbedBuilder ()
                    .WithTitle ("Insufficient permission")
                    .WithDescription ("You don't have the required permissions to run that command.")
                    .WithColor (Color.Red)
                    .Build ();

                switch (data.AllowedRoles)
                {
                    case UserType.Staff:
                        if (!UserHelper.HasStaffRole (s.Author as IGuildUser))
                        {
                            var messg = await context.Channel.SendMessageAsync (string.Empty, false, eb);
                            _ = messg.TimedDeletion(5000);
                            return;
                        }
                        break;
                    case UserType.StaffGuru:
                        if (!UserHelper.HasStaffRole (s.Author as IGuildUser) && 
                            !UserHelper.HasRole (s.Author as IGuildUser, _settings ["guru-role"]))
                        {
                            var messg = await context.Channel.SendMessageAsync (string.Empty, false, eb);
                            _ = messg.TimedDeletion(5000);
                            return;
                        } 
                        break;
                }
            }            

            bool cooldownCommand = CheckIfCommandHasCooldown (executedCommand.Name.ToLower ());

            bool sameParamCommand = false; 

            string parameters = "";

            bool cooldownExpired = false;

            if (cooldownCommand && !UserHelper.HasStaffRole (s.Author as IGuildUser))
            {
                sameParamCommand = CheckIfSameParameterCommand (executedCommand.Name.ToLower ());    
                parameters = s.ToString ().Remove (0, s.ToString ().IndexOf (' ') + 1);
                cooldownExpired = HasCooldownExpired (executedCommand.Name, s.Author as IGuildUser, sameParamCommand, parameters);            
                if (!cooldownExpired)
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

                IMessage errorMsg = await context.Channel.SendMessageAsync(string.Empty, false, builder.Build());
                _ = errorMsg.TimedDeletion(3000);
            }
            else
            {
                if (cooldownCommand && !UserHelper.HasStaffRole (s.Author as IGuildUser))
                    AddUserToCooldown (executedCommand.Name, s.Author as IGuildUser, sameParamCommand, parameters);
                string command = executedCommand.Name;
                if(_statistics.Has(command))
                {
                    _statistics.Set(command, _statistics.Get(command) + 1);
                }
                else 
                {
                    _statistics.Add(command, 1);
                }
            }
        }

        /// <summary>
        /// Ensures the existance of pre-data files.
        /// </summary>
        private void EnsurePreDataFiles()
        {
            const string templatePrefix = "template-";
            int prefixLength = templatePrefix.Length;

            foreach (string templateFile in templateFiles)
            {
                string filename = templateFile.Substring(prefixLength);
                if (!File.Exists(filename))
                {
                    File.Copy(templateFile, filename);
                }
            }
        }

        /// <summary>
        /// Checks if the command with the specified name has a cooldown.
        /// </summary>
        private bool CheckIfCommandHasCooldown (string commandName)
        {
            if (_cooldowns.Commands.Any (c => c.CommandName == commandName)) return true;
            if (_cooldowns.SameParameterCommands.Any (c => c.CommandName == commandName)) return true;
            return false;
        }

        /// <summary>
        /// Checks if the command with the specified name is a same parameter command.
        /// </summary>
        private bool CheckIfSameParameterCommand (string commandName)
        {
            if (_cooldowns.Commands.Any (c => c.CommandName == commandName)) return false;
            if (_cooldowns.SameParameterCommands.Any (c => c.CommandName == commandName)) return true;
            throw new ArgumentException ("Command isn't a normal command nor a same parameter command.");
        }

        /// <summary>
        /// Returns the <see cref="TimeSpan"/> until the specified command has expired (for the specified user).
        /// </summary>
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

        /// <summary>
        /// Gets the cooldown until a certain user can be targeted again.
        /// </summary>
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

            _cooldowns.Save("cooldowns.json");
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
            if (_settings.Has("job-channel-ids"))
            {
                ulong[] ignoreChannelIds = _settings["job-channel-ids"].Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
                if (ignoreChannelIds.Any(id => id == s.Channel.Id)) return;
            }

            await PasteCommand.PasteIfMassiveCodeblock(s);
        }
    }
}
