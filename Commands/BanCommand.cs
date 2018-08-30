using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands.Moderation
{
    public class BanCommand : ModuleBase
    {

        /*
        Todo: Add duration functionality
         */
        [Command("ban")]
        [HelpData("ban <member> <reason>", "Ban a member.", HelpMode = "mod")]
        public async Task Ban(IGuildUser user, [Optional] [Remainder] string reason)
        {
            (Context.User as IGuildUser).EnsureStaff();

            await user.BanAsync(reason: reason);
        }
    }
}
