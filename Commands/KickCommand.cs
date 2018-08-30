using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands.Moderation
{
    public class KickCommand : ModuleBase
    {
        [Command("kick")]
        [HelpData("kick <member> <reason>", "Kick a member.", HelpMode = "mod")]
        public async Task Kick(IGuildUser user, [Optional] [Remainder] string reason)
        {
            (Context.User as IGuildUser).EnsureStaff();

            await user.KickAsync(reason);
        }
    }
}
