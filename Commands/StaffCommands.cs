using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class StaffCommands : ModuleBase
    {
        private readonly CommandService commands;

        public StaffCommands(CommandService commands)
        {
            this.commands = commands;
        }

        [Command("set")]
        public async Task ApplySetting(string name, string value)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            SettingsTable settings = BrackeysBot.Settings;
            settings[name] = value;

            await ReplyAsync("Settings have been applied.");
        }
    }
}
