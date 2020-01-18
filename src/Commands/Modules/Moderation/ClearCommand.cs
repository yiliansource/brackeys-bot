using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public partial class ModerationModule : BrackeysBotModule
    {
        [Command("clear")]
        [Summary("Deletes a specified amount of messages from the channel.")]
        [Remarks("clear <count>")]
        [RequireModerator]
        [RequireContext(ContextType.Guild)]
        public async Task ClearMessagesAsync(
            [Summary("The amount of messages to clear")] int count)
        {
            var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
            var validMessages = messages.Where(m => (DateTimeOffset.Now - m.CreatedAt).Days < 14);
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(validMessages);

            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.ClearMessages)
                .WithChannel(Context.Channel as ITextChannel));
        }
    }
}
