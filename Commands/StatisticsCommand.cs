using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using BrackeysBot.Data;

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
        [HelpData("statistics", "Displays the most frequently used commands.", AllowedRoles = UserType.Staff)]
        [Alias("stats")]
        public async Task DisplayStatistics()
        {
            int.TryParse(_settings["leaderboard-pagesize"], out int pagesize);

            var stats = _statisticsTable.GetSortedStatistics();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here are the command usage statistics:");
            sb.AppendLine();

            foreach (KeyValuePair<string, uint> stat in stats) 
            {
                sb.AppendLine($"{ stat.Key } was used { stat.Value } times.");
            }

            var message = await ReplyAsync(sb.ToString());
        }
    }
}
