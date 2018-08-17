using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class PointsCommand : ModuleBase
    {
        private readonly CommandService commands;

        public PointsCommand(CommandService commands)
        {
            this.commands = commands;
        }

        [Command("points")]
        public async Task DisplayPointsSelf ()
        {
            var user = Context.User;
            int karma = BrackeysBot.Karma.GetKarma(user);
            await ReplyAsync($"{ user.Mention }, you have { karma } points.");
        }

        [Command("points")]
        public async Task DisplayPointsUser (SocketGuildUser user)
        {
            int remainingSeconds;
            if (KarmaTable.CheckPointsUserCooldownExpired(Context.User, out remainingSeconds))
            {
                int total = BrackeysBot.Karma.GetKarma(user);
                string pointsDisplay = $"{ total } point{ (total != 1 ? "s" : "") }";
                await ReplyAsync($"{ user.Username } has { pointsDisplay }.");

                // Make sure that staff doesn't have cooldowns
                if (!StaffCommandHelper.HasStaffRole(Context.User as IGuildUser))
                {
                    KarmaTable.AddPointsUserCooldown(Context.User);
                }
            }
            else
            {
                string displaySeconds = $"{ remainingSeconds } second{ (remainingSeconds != 1 ? "s" : "") }";
                await ReplyAsync($"{ Context.User.Mention }, please wait { displaySeconds } before using that command again.");
            }
        }
    }
}
