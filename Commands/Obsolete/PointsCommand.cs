using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    class PointsCommand : ModuleBase
    {
        private readonly KarmaTable _karmaTable;
        private readonly SettingsTable _settings;

        public PointsCommand(KarmaTable karmaTable, SettingsTable settingsTable)
        {
            _karmaTable = karmaTable;
            _settings = settingsTable;
        }
        
        [Command("points")]
        [HelpData("points", "Displays your points.")]
        public async Task DisplayPointsSelf ()
        {
            var user = Context.User;
            int karma = _karmaTable.GetKarma(user);

            string pointsDisplay = $"{ karma } point{ (karma != 1 ? "s" : "") }";
            await ReplyAsync($"{ user.Mention }, you have { pointsDisplay }.");
        }

        [Command("rank")]
        [HelpData("rank", "Displays your current leaderboard rank.")]
        public async Task DisplayRankSelf ()
        {
            var user = Context.User;
            
            int karma = _karmaTable.GetKarma(user);
            if (karma == 0)
            {
                await ReplyAsync("You aren't on the leaderboard yet!");
            }
            else
            {
                int index = _karmaTable.GetSortedLeaderboard().ToList().IndexOf(new System.Collections.Generic.KeyValuePair<ulong, int>(user.Id, karma));
                await ReplyAsync($"{ user.Mention }, you are currently in place { index + 1 }.");
            }
        }

        [Command("points")]
        [HelpData("points <user>", "Displays another users points.")]
        public async Task DisplayPointsUser (SocketGuildUser user)
        {
            int total = _karmaTable.GetKarma(user);
            string pointsDisplay = $"{ total } point{ (total != 1 ? "s" : "") }";
            await ReplyAsync($"{ UserHelper.GetDisplayName(user) } has { pointsDisplay }.");
        }
    }
}
