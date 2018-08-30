using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands.Moderation
{
    public class ClearCommand : ModuleBase
    {
        [Command("clear")]
        [HelpData("clear <amount of messages>", "Clears the specified amount of messages.", HelpMode = "mod")]
        public async Task Clear(int amount)
        {
            (Context.User as IGuildUser).EnsureStaff();
            var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();

            foreach (IMessage message in messages)
            {
                await message.DeleteAsync();
            }
        }
    }
}
