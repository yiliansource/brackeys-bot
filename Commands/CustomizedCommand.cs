using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using System.Text;

namespace BrackeysBot.Commands
{
    public class CustomizedCommand : ModuleBase
    {
        private readonly CustomCommandsTable _customCommands;

        public CustomizedCommand(CustomCommandsTable customCommands)
        {
            _customCommands = customCommands;
        }

        [Command("ccadd")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ccadd <name> <message>", "Adds a command that prints the specified message when called.")]
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
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ccdelete <name>", "Deletes the specified custom command.")]
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

        [Command("ccclear")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ccclear", "Deletes all custom commands.")]
        public async Task ClearCustomCommands()
        {
            _customCommands.Clear();
        }
    }
}
