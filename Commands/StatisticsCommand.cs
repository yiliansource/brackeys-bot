using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class StatisticsCommand : ModuleBase
    {
        private readonly StatisticsTable _statisticsTable;
        private readonly SettingsTable _settings;

        public StatisticsCommand(StatisticsTable statisticsTable, SettingsTable settings)
        {
            _statisticsTable = statisticsTable;
            _settings = settings;
        }

        [Command("statistics")]
        [HelpData("statistics", "Displays the most frequently used commands.", HelpMode = "mod")]
        [Alias("stats")]
        public async Task DisplayStatistics()
        {
            (Context.User as IGuildUser).EnsureStaff();

            int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

            var stats = _statisticsTable.GetSortedStatistics();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here are the statistics for the command usages:");
            sb.AppendLine();

            foreach (KeyValuePair<string, uint> stat in stats) 
            {
                sb.AppendLine($"{ stat.Key } was used { stat.Value } times.");
            }

            var message = await ReplyAsync(sb.ToString());
        }
    }
}
