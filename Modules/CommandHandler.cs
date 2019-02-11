using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Modules
{
    /// <summary>
    /// Provides a module to execute commands.
    /// </summary>
    public class CommandHandler
    {
        public CommandService Service => _commandService;
        private CommandService _commandService;

        public IServiceProvider ServiceProvider { get; set; }
        public string CommandPrefix { get; set; }
        
        private DataModule _data;
        private CustomCommandModule _customCommandModule;

        public CommandHandler(DataModule data, string commandPrefix)
        {
            CommandPrefix = commandPrefix;

            _data = data;
            _commandService = new CommandService();
            _customCommandModule = new CustomCommandModule(data.CustomCommands);
        }

        /// <summary>
        /// Installs the command handling to the client.
        /// </summary>
        public async Task InstallCommands(DiscordSocketClient client)
        {
            client.MessageReceived += (s) => HandleCommand(s, client);
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Handles a command, represented in the given message.
        /// </summary>
        private async Task HandleCommand(SocketMessage s, DiscordSocketClient client)
        {
            if (!(s is SocketUserMessage msg)) return;

            int argPos = 0;
            if (!msg.HasStringPrefix(CommandPrefix, ref argPos)) return;

            IGuildUser author = s.Author as IGuildUser;
            CommandContext context = new CommandContext(client, msg);

            Log.WriteLine($"{msg.Author} attempted to invoke \"{msg.Content}\".");

            if (context.IsPrivate)
            {
                // Don't allow bot usage in private messages
                await context.Channel.SendMessageAsync ("I'm sorry but you can't use commands here since they don't work in DMs (not my fault, I swear :eyes:). Please run the commands in our server! :smile:");
                return;
            }

            CommandInfo executedCommand = null;
            try
            {
                executedCommand = _commandService.Search(context, argPos).Commands.First().Command;
            }
            catch
            {
                // The executed command wasnt found in the modules, therefore look if any custom commands are registered.
                string commandName = msg.Content.Substring(argPos).Split(' ')[0].ToLowerInvariant();
                CustomCommand command = _customCommandModule.FindCommand(commandName);
                if (command != null)
                {
                    // Attempt to execute the custom command
                    try { await command.Execute(author, context.Channel); }
                    // If an exception occurs, print it
                    catch (Exception ex)
                    {
                        EmbedBuilder eb = new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithTitle("Error")
                            .WithDescription(ex.Message);

                        await context.Channel.SendMessageAsync(string.Empty, false, eb);
                    }
                    return;
                }
                else
                {
                    // Also no custom command was found? Check all commands and custom commands if there was a close match somewhere
                    IEnumerable<string> commandNames = _commandService.Commands
                        .Where(c => // Make sure that only commands that can be used by the user get listed
                        {
                            PermissionRestrictionAttribute pra = c.Attributes.FirstOrDefault(a => a is PermissionRestrictionAttribute) as PermissionRestrictionAttribute;
                            if (pra != null)
                            {
                                UserType roles = pra.AllowedRoles;

                                // Allow the command usage if anyone can use it, or the user is a staff member
                                if (roles.HasFlag(UserType.Everyone) || author.HasStaffRole()) { return true; }
                                // If the command is for gurus, check if the user has the guru role
                                if (roles.HasFlag(UserType.Guru)) { return author.HasGuruRole(); }

                                return false;
                            }
                            return true;
                        })
                        .Select(c => c.Name)
                        .Concat(_data.CustomCommands.CommandNames)
                        .Distinct();

                    const int LEVENSHTEIN_TOLERANCE = 2;

                    string closeMatch = commandNames
                        .ToDictionary(l => l, l => commandName.ToLowerInvariant().ComputeLevenshtein(l.ToLowerInvariant())) // Use lower casing to avoid too high intolerance
                        .Where(l => l.Value <= LEVENSHTEIN_TOLERANCE)
                        .OrderBy(l => l.Value)
                        .FirstOrDefault().Key;

                    if (closeMatch != null)
                    {
                        // A close match was found! Notify the user.
                        EmbedBuilder eb = new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithTitle("Error")
                            .WithDescription($"The command \"{command}\" could not be found. Did you mean \"{closeMatch}\"?");

                        await context.Channel.SendMessageAsync(string.Empty, false, eb);
                    }

                    // The entered command was just nonsense. Just ignore it.
                    return;
                }
            }

            PermissionRestrictionAttribute restriction = executedCommand.Attributes.FirstOrDefault(a => a is PermissionRestrictionAttribute) as PermissionRestrictionAttribute;
            if (restriction != null)
            {
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle("Insufficient permissions")
                    .WithDescription("You don't have the required permissions to run that command.")
                    .WithColor(Color.Red);

                bool denyInvokation = true;
                UserType roles = restriction.AllowedRoles;
                
                // Allow the command usage if anyone can use it, or the user is a staff member
                if (roles.HasFlag(UserType.Everyone) || author.HasStaffRole()) { denyInvokation = false; }
                // If the command is for gurus, check if the user has the guru role
                if (roles.HasFlag(UserType.Guru) && author.HasGuruRole()) { denyInvokation = false; }

                if (denyInvokation)
                {
                    var message = await context.Channel.SendMessageAsync(string.Empty, false, eb);
                    _ = message.TimedDeletion(5000);
                    Log.WriteLine($"The command \"{msg.Content}\" failed with the reason InsufficientPermissions.");
                    return;
                }
            }

            bool cooldownCommand = CheckIfCommandHasCooldown(executedCommand.Name.ToLower());
            bool sameParamCommand = false;
            bool cooldownExpired = false;
            string parameters = "";

            if (cooldownCommand && !UserHelper.HasStaffRole(s.Author as IGuildUser))
            {
                sameParamCommand = CheckIfSameParameterCommand(executedCommand.Name.ToLower());
                parameters = s.ToString().Remove(0, s.ToString().IndexOf(' ') + 1);
                cooldownExpired = HasCooldownExpired(executedCommand.Name, s.Author as IGuildUser, sameParamCommand, parameters);
                if (!cooldownExpired)
                {
                    TimeSpan ts = GetTimeUntilCooldownHasExpired(executedCommand.Name.ToLower(), s.Author as IGuildUser, sameParamCommand, parameters);

                    Embed eb = new EmbedBuilder()
                        .WithTitle("Cooldown hasn't expired yet")
                        .WithDescription($"{s.Author.Mention}, you can't run this command yet. Please wait {ts.Hours} hours, {ts.Minutes} minutes and {ts.Seconds} seconds.")
                        .WithColor(Color.Orange);

                    await context.Channel.SendMessageAsync(string.Empty, false, eb);
                    return;
                }
            }

            IResult result = await _commandService.ExecuteAsync(context, argPos, ServiceProvider);
            if (!result.IsSuccess)
            {
                Log.WriteLine($"The command \"{msg.Content}\" failed with the reason {result.Error.Value}: \"{result.ErrorReason}\"");

                if (result.Error == CommandError.UnknownCommand
                    || result.Error == CommandError.BadArgCount 
                    || result.Error == CommandError.ParseFailed)
                {
                    return;
                }

                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription(result.ErrorReason)
                    .WithColor(Color.Red);

                IMessage errorMsg = await context.Channel.SendMessageAsync(string.Empty, false, builder.Build());
            }
            else
            {
                if (cooldownCommand && !UserHelper.HasStaffRole(s.Author as IGuildUser))
                    AddUserToCooldown(executedCommand.Name, s.Author as IGuildUser, sameParamCommand, parameters);
                string command = executedCommand.Name;
                if (_data.Statistics.Has(command))
                {
                    _data.Statistics.Set(command, _data.Statistics.Get(command) + 1);
                }
                else
                {
                    _data.Statistics.Add(command, 1);
                }
            }
        }

        /// <summary>
        /// Checks if the command with the specified name has a cooldown.
        /// </summary>
        private bool CheckIfCommandHasCooldown(string commandName)
        {
            if (_data.Cooldowns.Commands.Any(c => c.CommandName == commandName)) return true;
            if (_data.Cooldowns.SameParameterCommands.Any(c => c.CommandName == commandName)) return true;
            return false;
        }

        /// <summary>
        /// Checks if the command with the specified name is a same parameter command.
        /// </summary>
        private bool CheckIfSameParameterCommand(string commandName)
        {
            if (_data.Cooldowns.Commands.Any(c => c.CommandName == commandName)) return false;
            if (_data.Cooldowns.SameParameterCommands.Any(c => c.CommandName == commandName)) return true;
            throw new ArgumentException("Command isn't a normal command nor a same parameter command.");
        }

        /// <summary>
        /// Returns the <see cref="TimeSpan"/> until the specified command has expired (for the specified user).
        /// </summary>
        private TimeSpan GetTimeUntilCooldownHasExpired(string commandName, IGuildUser user, bool sameParameters, string parameters = "")
        {
            if (sameParameters && !string.IsNullOrEmpty(parameters))
            {
                CommandCooldown<UserCooldownParameters> sameParamCool = _data.Cooldowns.SameParameterCommands.FirstOrDefault(c => c.CommandName == commandName);
                UserCooldownParameters usrCool = sameParamCool.Users.FirstOrDefault(u => u.Id == user.Id && u.Parameters == parameters);

                DateTime executedTime = usrCool.CommandExecutedTime.ToDateTime();
                DateTime currentTime = DateTime.UtcNow;

                return executedTime.AddSeconds(sameParamCool.CooldownTime) - currentTime;
            }
            else
            {
                CommandCooldown<UserCooldown> cmdCool = _data.Cooldowns.Commands.FirstOrDefault(c => c.CommandName == commandName);
                UserCooldown usrCool = cmdCool.Users.FirstOrDefault(u => u.Id == user.Id);

                DateTime executedTime = usrCool.CommandExecutedTime.ToDateTime();
                DateTime currentTime = DateTime.UtcNow;

                return executedTime.AddSeconds(cmdCool.CooldownTime) - currentTime;
            }
        }

        /// <summary>
        /// Gets the cooldown until a certain user can be targeted again.
        /// </summary>
        private T GetUserCooldown<T>(string commandName, IGuildUser user, bool sameParameters = false, string parameters = "") where T : UserCooldown
        {
            if (sameParameters && !string.IsNullOrEmpty(parameters))
            {
                CommandCooldown<UserCooldownParameters> sameParamCool = _data.Cooldowns.SameParameterCommands.FirstOrDefault(c => c.CommandName == commandName);

                // If the command couldn't be found in same parameter cooldowns list then the command has no cooldown options
                if (sameParamCool == null) return null;

                UserCooldownParameters usrCool = sameParamCool.Users.FirstOrDefault(u => u.Id == user.Id && u.Parameters == parameters);

                // This user isn't listed yet, meaning the cooldown has either expired or the user hasn't ran the command yet
                if (usrCool == null) return null;

                return (T)((UserCooldown)usrCool);
            }
            else if (sameParameters && string.IsNullOrEmpty(parameters))
            {
                throw new ArgumentException("Parameters are empty, and checking for same parameter cooldowns.");
            }
            else
            {
                CommandCooldown<UserCooldown> cmdCool = _data.Cooldowns.Commands.FirstOrDefault(c => c.CommandName == commandName);

                // If the command couldn't be found, then search in the same parameter cooldowns list
                if (cmdCool == null) return null;

                UserCooldown usrCool = cmdCool.Users.FirstOrDefault(u => u.Id == user.Id);

                // This user isn't listed yet, meaning the cooldown has either expired or the user hasn't ran the command yet.
                if (usrCool == null) return null;

                return (T)usrCool;
            }
        }

        /// <summary>
        /// Checks if the cooldown expired.
        /// </summary>
        private bool HasCooldownExpired(string commandName, IGuildUser user, bool sameParameters = false, string parameters = "")
        {
            if (GetUserCooldown<UserCooldown>(commandName, user, sameParameters, parameters) == null) return true;
            if (sameParameters)
                if (GetUserCooldown<UserCooldownParameters>(commandName, user, sameParameters, parameters) == null) return true;

            TimeSpan ts = GetTimeUntilCooldownHasExpired(commandName, user, sameParameters, parameters);
            if (ts.Ticks < 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Adds a user to the cooldown list. If he already exists, the user's executed time is just changed.
        /// </summary>
        private void AddUserToCooldown(string commandName, IGuildUser user, bool sameParameters = false, string parameters = "")
        {
            if (sameParameters && !string.IsNullOrEmpty(parameters))
            {
                CommandCooldown<UserCooldownParameters> cmdCool = _data.Cooldowns.SameParameterCommands.FirstOrDefault(c => c.CommandName == commandName);
                UserCooldownParameters usrCool = GetUserCooldown<UserCooldownParameters>(commandName, user, true, parameters);
                if (usrCool == null)
                    cmdCool.Users.Add(new UserCooldownParameters { Id = user.Id, CommandExecutedTime = DateTime.UtcNow.ToTimestamp(), Parameters = parameters });
                else
                    usrCool.CommandExecutedTime = DateTime.UtcNow.ToTimestamp();
            }
            else
            {
                CommandCooldown<UserCooldown> cmdCool = _data.Cooldowns.Commands.FirstOrDefault(c => c.CommandName == commandName);
                UserCooldown usrCool = GetUserCooldown<UserCooldown>(commandName, user, false, "");
                if (usrCool == null) { cmdCool.Users.Add(new UserCooldown { Id = user.Id, CommandExecutedTime = DateTime.UtcNow.ToTimestamp() }); }
                else { usrCool.CommandExecutedTime = DateTime.UtcNow.ToTimestamp(); }
            }

            _data.Cooldowns.Save("cooldowns.json");
        }
    }
}
