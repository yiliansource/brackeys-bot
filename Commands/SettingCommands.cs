using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class SettingCommands : ModuleBase
    {
        private readonly SettingsTable _settings;

        public SettingCommands(SettingsTable settings)
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
                await ReplyAsync("I updated the setting for you!");
            }
            else
            {
                _settings.Add(name, value);
                await ReplyAsync("I added the setting for you!");
            }
        }

        [Command("delete"), Alias("del")]
        [HelpData("delete <name>", "Deletes a setting.", AllowedRoles = UserType.Staff)]
        public async Task DeleteSetting(string name)
        {
            if (_settings.Has(name))
            {
                _settings.Remove(name);
                await ReplyAsync("I removed the setting for you!");
            }
            else
            {
                await ReplyAsync("There is no such setting!");
            }
        }

        [Command("viewsettings"), Alias("settings")]
        [HelpData("viewsettings", "Views all registered settings.", AllowedRoles = UserType.Staff)]
        public async Task ViewSettings() 
        {
            var allsettings = _settings.Settings;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine("Settings:");
            sb.AppendLine();

            foreach (KeyValuePair<string, string> setting in allsettings) 
            {
                sb.AppendLine($"\"{ setting.Key }\": \"{ setting.Value }\"");
            }

            sb.AppendLine("```");

            await ReplyAsync(sb.ToString());
        }
    }
}
