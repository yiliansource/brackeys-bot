using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Data;

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
        [HelpData("set <name> <value>", "Updates a setting.", AllowedRoles = UserType.Staff)]
        public async Task ApplySetting(string name, [Remainder]string value)
        {
            (Context.User as IGuildUser).EnsureStaff();

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
