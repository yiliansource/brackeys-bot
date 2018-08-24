using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.Configuration;

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
            EmbedBuilder helpDialog = GetHelpDialog("default");
            await ReplyAsync(string.Empty, false, helpDialog.Build());
        }

        [Command("modhelp")]
        [HelpData("modhelp", "Displays this menu.", HelpMode = "mod")]
        public async Task ModHelp ()
        {
            (Context.User as IGuildUser).EnsureStaff();

            EmbedBuilder helpDialog = GetHelpDialog("mod");
            await ReplyAsync(string.Empty, false, helpDialog.Build());
        }

        /// <summary>
        /// Returns the help dialog for a specific mode.
        /// </summary>
        private EmbedBuilder GetHelpDialog(string mode)
        {
            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(247, 22, 131))
                .WithTitle("BrackeysBot")
                .WithDescription("The official Brackeys server bot. Commands are:");

            string prefix = _configuration["prefix"];

            var commands = GetCommandDataCollection(mode);
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
        private IEnumerable<HelpDataAttribute> GetCommandDataCollection (string mode)
        {
            var commandList = new List<HelpDataAttribute>();

            foreach (ModuleInfo module in _commands.Modules)
                foreach (CommandInfo command in module.Commands)
                {
                    if (command.Attributes.FirstOrDefault(a => a is HelpDataAttribute) is HelpDataAttribute data)
                    {
                        if (mode.ToLower() == data.HelpMode.ToLower())
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
