using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace BrackeysBot.Commands
{
    public class TemplateCommand : ModuleBase
    {
        private readonly static Regex _jobRegex = new Regex(@"(```.*\[Hiring\]\n--------------------------------\nProject Name: .*\nRole Required: .*\nMy Previous Projects / Portfolio \(N/A if none\): .*\nTeam Size: .*\nProject Length \(specify if it's not strict\): .*\nCompensation: .*\nResponsibilities: .*\nProject Description: .*```)|(```.*\[Looking for work\]\n--------------------------------\nMy Role: .*\nSkills: .*\nMy Previous Projects / Portfolio \(N/A if none\): .*\nExperience in field: .*\nRates: .*```)|(```.*\[Hiring\]\n--------------------------------\nProject Name: .*\nRole Required: .*\nMy Previous Projects / Portfolio \(N/A if none\): .*\nProject Description: .*```)|(```.*\[Looking for work\]\n--------------------------------\nMy Role: .*\nSkills: .*\nMy Previous Projects / Portfolio \(N/A if none\): .*```)|(```.*\[Recruiting\]\n--------------------------------\nProject Name: .*\nProject Description: .*```)|\n(```.*\[Looking to mentor\]\n--------------------------------\nAre of interest: .*\nRates / Free: .*```)|(```.*\[Looking for a mentor\]\n--------------------------------\nArea of interest: .*\nRates / Free: .*```)", RegexOptions.Compiled | RegexOptions.Singleline);
        private readonly SettingsTable _settings;

        public TemplateCommand(SettingsTable settingsTable)
        {
            _settings = settingsTable;
        }
        [Command("template")]
        [HelpData("template", "Ensures a template is followed for all the messages in the Jobs category.", HelpMode = "mod")]
        public async Task EnsureTemplate()
        {
            (Context.User as IGuildUser).EnsureStaff();

            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten();
            foreach (IMessage message in messages)
            {
                await CheckTemplate(message, _settings);
            }
        }

        public static async Task CheckTemplate(IMessage s, SettingsTable _settings)
        {
            ulong[] ignoreChannelIds = _settings["massivecodeblock-ignore"].Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
            if (ignoreChannelIds.All(id => id != s.Channel.Id)) return;
            if (!_jobRegex.IsMatch(s.Content))
            {
                if (!s.Author.IsBot)
                    await s.Author.SendMessageAsync($"Hi, {s.Author.Username}. I've removed the message you've sent in #{s.Channel.Name} at {s.Timestamp.DateTime.ToString()} UTC, because you didn't follow the template. Please re-post it using the provided template that is pinned to that channel.");
                await s.DeleteAsync();
                await Task.Delay(1000);
            }
        }
    }
}
