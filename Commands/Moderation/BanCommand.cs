using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord.Commands;
using Discord;

namespace BrackeysBot.Commands.Moderation
{
    public class BanCommand : ModuleBase
    {

        [Command("ban")]
        [HelpData("ban <member> <reason> (optional)", "Ban a member.", AllowedRoles = UserType.Staff)]
        public async Task Ban(IGuildUser user, [Optional] [Remainder] string reason)
        {
            string _displayName = user.GetDisplayName();
            await Context.Guild.AddBanAsync(user, 7, reason);
            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully banned {_displayName}.");
            _ = messageToDel.TimedDeletion(3000);
        }
    }
}
