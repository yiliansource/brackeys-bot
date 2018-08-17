using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class HelpCommand : ModuleBase
    {
        private readonly CommandService commands;

        public HelpCommand(CommandService commands)
        {
            this.commands = commands;
        }

        [Command ("help")]
        public async Task Help ()
        {
            EmbedBuilder helpDialog = GetHelpDialog("default");
            await ReplyAsync(string.Empty, false, helpDialog);
        }

        [Command("modhelp")]
        public async Task ModHelp ()
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            EmbedBuilder helpDialog = GetHelpDialog("mod");
            await ReplyAsync(string.Empty, false, helpDialog);
        }

        private EmbedBuilder GetHelpDialog(string mode)
        {
            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(new Color(247, 22, 131))
                .WithTitle("BrackeysBot")
                .WithDescription("The official Brackeys server bot. Commands are:");

            string prefix = BrackeysBot.Configuration["prefix"];

            List<KeyValuePair<string, string>> commands = BrackeysBot.Help.GetCommands(mode);
            foreach (KeyValuePair<string, string> command in commands)
            {
                eb.AddField(prefix + command.Key, command.Value);
            }

            return eb;
        }
    }
}
