using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using System.Text;

namespace BrackeysBot.Commands
{
    public class CustomizedCommand : ModuleBase
    {
        private CustomizedCommandTable _customCommands;

        public CustomizedCommand(CustomizedCommandTable customCommands)
        {
            _customCommands = customCommands;
        }

        [Command("ccadd")]
        [HelpData("ccadd <name> <message>", "Adds a command that prints the specified message when called.", AllowedRoles = Data.UserType.Staff)]
        public async Task AddCustomCommand (string name, [Remainder]string message)
        {
            if (_customCommands.Has(name))
            {
                _customCommands.Set(name, message);
            }
            else
            {
                _customCommands.Add(name, message);
            }
            await ReplyAsync("Custom command updated.");
        }

        [Command("ccdelete")]
        [HelpData("ccdelete <name>", "Deletes the specified custom command.", AllowedRoles = Data.UserType.Staff)]
        public async Task DeleteCustomCommand (string name)
        {
            if (_customCommands.Has(name))
            {
                _customCommands.Remove(name);

                await ReplyAsync("Custom command removed.");
                return;
            }
            else
            {
                await ReplyAsync("Custom command does not exist.");
            }
        }

        [Command("cclist")]
        [HelpData("cclist", "Lists all registered custom commands", AllowedRoles = Data.UserType.Staff)]
        public async Task ListCustomCommands()
        {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithTitle("Custom Commands");

            StringBuilder commands = new StringBuilder();
            foreach (string command in _customCommands.CommandNames)
            {
                commands.AppendLine($"{command}");
            }
            eb.WithDescription(commands.ToString());

            await ReplyAsync(string.Empty, false, eb);
        }
    }
}
