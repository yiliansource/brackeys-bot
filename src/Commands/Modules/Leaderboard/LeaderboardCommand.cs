using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public partial class LeaderboardModule : BrackeysBotModule
    {
        [Command("leaderboard"), Alias("top", "lb")]
        public async Task DisplayLeaderboardAsync()
        {
            await GetDefaultBuilder()
                .WithTitle("Leaderboard")
                .WithFields(Leaderboard.GetLeaderboard()
                    .Select((l, i) => new EmbedFieldBuilder { IsInline = true, Name = (i + 1).ToString().Envelop("**"), Value = $"{l.User.Mention} · {l.Points} points" }))
                .Build()
                .SendToChannel(Context.Channel);
        }
    }
}
