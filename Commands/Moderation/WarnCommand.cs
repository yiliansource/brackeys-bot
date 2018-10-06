using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Discord.Commands;
using Discord;
namespace BrackeysBot.Commands.Moderation
{
    public class WarnCommand : ModuleBase
    {
        private readonly WarningTable _warns;
        public WarnCommand(WarningTable warns)
        {
            _warns = warns;
        }
        [Command("warn")]
        [HelpData("warn <member> <severity (how many infraction points to give)> <reason> (optional)", "Warns a user.", AllowedRoles = UserType.Staff)]
        public async Task Warn(IGuildUser user, int severity, [Optional] [Remainder] string reason)
        {
            string warner = ((IGuildUser)Context.Message.Author).GetDisplayName();
            string _keyName = user.Id.ToString() + "," + Context.Guild.Id.ToString();

            WarningData data = new WarningData
            {
                time = DateTime.UtcNow.ToBinary(),
                severity = severity,
                warner = warner,
                reason = reason
            };
            var list = _warns.GetOrDefault(_keyName);
            if (list == null)
                list = new List<WarningData>();
            list.Add(data);
            _warns.Set(_keyName, list);

            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully warned {user.GetDisplayName()} for \"{reason}\".");
            _ = messageToDel.TimedDeletion(3000);
            await Context.Message.DeleteAsync();
        }
    }
}