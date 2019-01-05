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

        [Command("statistics"), Alias("stats")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("statistics", "Displays the most frequently used commands.")]
        public async Task DisplayStatistics()
        {
            int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

            var stats = _statisticsTable.GetSortedStatistics()
                .GroupBy(c => c.Key.ToLowerInvariant(), (c, e) => new KeyValuePair<string, uint>(c, (uint)e.Sum(s => s.Value)))
                .OrderByDescending(c => c.Value);

            const string commandLabel = "Command";
            const string countLabel = "Count";
            
            int longestStat = Math.Max(commandLabel.Length, stats.Max(s => s.Key.Length));

            string tableRow = commandLabel + new string(' ', 1 + (longestStat - commandLabel.Length)) + "| " + countLabel;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(tableRow);
            sb.AppendLine(new string('-', tableRow.Length).Insert(1 + longestStat, "+").Remove(tableRow.Length - 1));

            foreach (KeyValuePair<string, uint> stat in stats) 
            {
                string line = stat.Key + new string(' ', 1 + (longestStat - stat.Key.Length)) + "| " + stat.Value;
                sb.AppendLine(line);
            }

            await ReplyAsync("Here are the command usage statistics!\r\n```" + Environment.NewLine + sb.ToString() + Environment.NewLine + "```");
        }
    }
}
