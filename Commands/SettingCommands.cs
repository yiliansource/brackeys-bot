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

        [Command("setting"), Alias("set")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("setting <name> <value>", "Updates a setting.")]
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
        [PermissionRestriction(UserType.Staff)]
        [HelpData("delete <name>", "Deletes a setting.")]
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
        [PermissionRestriction(UserType.Staff)]
        [HelpData("viewsettings", "Views all registered settings.")]
        public async Task ViewSettings() 
        {
            var allsettings = _settings.Settings;
            const int maxMessageLength = 2000;

            StringBuilder settingsVisual = new StringBuilder();
            settingsVisual.AppendLine("```");
            settingsVisual.AppendLine("Settings:");
            settingsVisual.AppendLine();

            int length = settingsVisual.ToString().Length;

            foreach (KeyValuePair<string, string> setting in allsettings) 
            {
                string visual = $"\"{ setting.Key }\": \"{ setting.Value }\"";
                if (length + visual.Length + 5 < maxMessageLength)
                {
                    settingsVisual.AppendLine(visual);
                }
                else
                {
                    settingsVisual.AppendLine("```");
                    await ReplyAsync(settingsVisual.ToString());

                    settingsVisual.Clear();
                    settingsVisual.AppendLine("```");
                    settingsVisual.AppendLine(visual);

                    length = settingsVisual.Length;
                }
            }

            settingsVisual.AppendLine("```");

            await ReplyAsync(settingsVisual.ToString());
        }
    }
}
