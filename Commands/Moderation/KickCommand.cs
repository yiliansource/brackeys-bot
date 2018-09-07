using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;
using BrackeysBot.Data;

namespace BrackeysBot.Commands.Moderation
{
    public class KickCommand : ModuleBase
    {
        [Command("kick")]
        [HelpData("kick <member> <reason> (optional)", "Kick a member.", AllowedRoles = UserType.Staff)]
        public async Task Kick(IGuildUser user, [Optional] [Remainder] string reason)
        {
            (Context.User as IGuildUser).EnsureStaff();

            await user.KickAsync(reason);
            IMessage messageToDel = await ReplyAsync($":white_check_mark: {user.GetDisplayName()} kicked successfully.");
            _ = Task.Run(async () => await messageToDel.TimedDeletion(3000));
        }
    }
}
