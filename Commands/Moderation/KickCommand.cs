using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands.Moderation
{
    public class KickCommand : ModuleBase
    {
        [Command("kick")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("kick <member> <reason> (optional)", "Kick a member.")]
        public async Task Kick(IGuildUser user, [Optional] [Remainder] string reason)
        {
            string _displayName = user.GetDisplayName();
            await user.KickAsync(reason);
            IMessage messageToDel = await ReplyAsync($":white_check_mark: {_displayName} kicked successfully.");
            _ = messageToDel.TimedDeletion(3000);
        }
    }
}
