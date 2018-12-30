using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands.Moderation
{
    public class ClearCommand : ModuleBase
    {
        [Command("clear")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("clear <amount of messages> <sent by> (optional)", "Clears the specified amount of messages or clears the number of messages sent by a user, if specified.")]
        public async Task Clear(int amount, [Optional] IGuildUser user)
        {
            int count = 0;
            if (amount <= 0)
            {
                throw new System.Exception("Please provide a valid amount of messages to delete.");
            }
            await Context.Message.DeleteAsync();
            if (user == null)
            {
                var messages = await Context.Channel.GetMessagesAsync(amount).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
            }
            else
            {
                var messages = await Context.Channel.GetMessagesAsync(500).Flatten();
                var messagesForDeletion = new List<IMessage>();
                foreach (IMessage message in messages)
                {
                    if (message.Author == user && count < amount)
                    {
                        messagesForDeletion.Add(message);
                        count++;
                    }
                }
                await Context.Channel.DeleteMessagesAsync(messagesForDeletion);
            }

            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully cleared {((user == null) ? amount : count)} messages{((user != null) ? $" sent by {user.GetDisplayName()}" : string.Empty)}.");
            _ = messageToDel.TimedDeletion(3000);
        }
    }
}
