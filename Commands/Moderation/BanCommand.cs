using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord.Commands;
using Discord;

namespace BrackeysBot.Commands.Moderation
{
    public class BanCommand : ModuleBase
    {

        [Command("ban")]
        [HelpData("ban <member> <days of message history to delete> <reason> (optional)", "Ban a member and deletes the specified amount of days of their message history.", AllowedRoles = UserType.Staff)]
        public async Task Ban(IGuildUser user, int pruneDays, [Optional] [Remainder] string reason)
        {
            await Context.Guild.AddBanAsync(user, pruneDays, reason);
            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully banned {user.GetDisplayName()}.");
            _ = messageToDel.TimedDeletion(3000);
        }
    }
}
