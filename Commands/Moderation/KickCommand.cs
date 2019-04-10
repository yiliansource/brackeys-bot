using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

using BrackeysBot.Modules;

namespace BrackeysBot.Commands.Moderation
{
    public class KickCommand : ModuleBase
    {
        private AuditLog _auditLog;

        public KickCommand(AuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        [Command("kick")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("kick <member> <reason> (optional)", "Kick a member.")]
        public async Task Kick(IGuildUser user, [Optional] [Remainder] string reason)
        {
            string _displayName = user.GetDisplayName();
            await user.KickAsync(reason);
            IMessage messageToDel = await ReplyAsync($":white_check_mark: {_displayName} was kicked.");
            _ = messageToDel.TimedDeletion(3000);

            await _auditLog.AddEntry($"{_displayName} was kicked.");
        }
    }
}
