using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Discord;
using Discord.Commands;

using BrackeysBot.Data;

namespace BrackeysBot.Commands
{
    public class HelpCommand : ModuleBase
    {
        private readonly CommandService _commands;
        private readonly IConfiguration _configuration;

        public HelpCommand(CommandService commands, IConfiguration configuration)
        {
            _commands = commands;
            _configuration = configuration;
        }

        [Command ("help")]
        [HelpData("help", "Displays this menu.")]
        public async Task Help ()
        {
            EmbedBuilder helpDialog = GetHelpDialog(UserType.Everyone);
            await ReplyAsync(string.Empty, false, helpDialog);
        }

        [Command("modhelp")]
        [HelpData("modhelp", "Displays this menu.", AllowedRoles = UserType.Staff)]
        public async Task ModHelp ()
        {
            EmbedBuilder helpDialog = GetHelpDialog(UserType.Staff);
            await ReplyAsync(string.Empty, false, helpDialog);
        }

        /// <summary>
        /// Returns the help dialog for a specific mode.
        /// </summary>
        private EmbedBuilder GetHelpDialog(UserType userType)
        {
            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(247, 22, 131))
                .WithTitle("BrackeysBot")
                .WithDescription("The official Brackeys server bot. Commands are:");

            string prefix = _configuration["prefix"];

            var commands = GetCommandDataCollection(userType);
            foreach (var command in commands)
            {
                string title = prefix + command.Usage;
                string content = command.Description;
                eb.AddField(title, content);
            }

            return eb;
        }

        /// <summary>
        /// Gathers the data for all commands found in the command modules.
        /// </summary>
        private IEnumerable<HelpDataAttribute> GetCommandDataCollection (UserType userType)
        {
            var commandList = new List<HelpDataAttribute>();

            foreach (ModuleInfo module in _commands.Modules)
                foreach (CommandInfo command in module.Commands)
                {
                    if (command.Attributes.FirstOrDefault(a => a is HelpDataAttribute) is HelpDataAttribute data)
                    {
                        if (userType == data.AllowedRoles)
                        {
                            commandList.Add(data as HelpDataAttribute);
                        }
                    }
                }

            var ordered = commandList.OrderBy(data => data.ListOrder);
            return ordered;
        }
    }
}
