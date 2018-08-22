using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class PointsCommand : ModuleBase
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

            await ReplyAsync($"{ user.Mention }, you have { karma } points.");
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
