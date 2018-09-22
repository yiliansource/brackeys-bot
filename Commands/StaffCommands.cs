using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        [HelpData("set <name> <value>", "Updates a setting.", AllowedRoles = UserType.Staff)]
        public async Task ApplySetting(string name, [Remainder]string value)
        {
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

        [Command("viewsettings")]
        [HelpData("viewsettings", "Views all registered settings.", AllowedRoles = UserType.Staff)]
        public async Task ViewSettings() 
        {
            var allsettings = _settings.Settings;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Settings:");
            sb.AppendLine();

            foreach (KeyValuePair<string, string> setting in allsettings) 
            {
                sb.AppendLine($"\"{ setting.Key }\": \"{ setting.Value }\"");
            }

            await ReplyAsync(sb.ToString());
        }
    }
}
