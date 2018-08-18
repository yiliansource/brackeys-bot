using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class HelpCommand : ModuleBase
    {
        private readonly CommandService _commands;

        public HelpCommand(CommandService commands)
        {
            _commands = commands;
        }

        [Command ("help")]
        [HelpData("help", "Displays this menu.")]
        public async Task Help ()
        {
            EmbedBuilder helpDialog = GetHelpDialog("default");
            await ReplyAsync(string.Empty, false, helpDialog);
        }

        [Command("modhelp")]
        [HelpData("modhelp", "Displays this menu.", HelpMode = "mod")]
        public async Task ModHelp ()
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            EmbedBuilder helpDialog = GetHelpDialog("mod");
            await ReplyAsync(string.Empty, false, helpDialog);
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

            string prefix = "[]";

            var commands = GetCommandDatas(mode);
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
        private IEnumerable<HelpDataAttribute> GetCommandDatas (string mode)
        {
            var commandList = new List<HelpDataAttribute>();

            foreach (ModuleInfo module in _commands.Modules)
                foreach (CommandInfo command in module.Commands)
                {
                    var data = command.Attributes.FirstOrDefault(a => a is HelpDataAttribute);
                    if (data != default(HelpDataAttribute))
                    {
                        var helpData = data as HelpDataAttribute;
                        if (mode.ToLower() == helpData.HelpMode.ToLower())
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
