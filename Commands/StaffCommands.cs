using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class StaffCommands : ModuleBase
    {
        private readonly SettingsTable _settings;

        public StaffCommands(SettingsTable settings)
        {
            _settings = settings;
        }

        [Command("set")]
        [HelpData("set <name> <value>", "Updates a setting.", HelpMode = "mod")]
        public async Task ApplySetting(string name, string value)
        {
            StaffCommandHelper.EnsureStaff(Context.User as IGuildUser);

            if (_settings.Has(name))
            {
                _settings.Set(name, value);
            }
            else
            {
                _settings.Add(name, value);
            }

            await ReplyAsync("Setting has been applied.");
        }
    }
}
