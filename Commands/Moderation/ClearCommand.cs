using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using BrackeysBot.Data;

namespace BrackeysBot.Commands.Moderation
{
    public class ClearCommand : ModuleBase
    {
        [Command("clear")]
        [HelpData("clear <amount of messages> <sent by> (optional)", "Clears the specified amount of messages or clears the number of messages sent by a user, if specified.", AllowedRoles = UserType.Staff)]
        public async Task Clear(int amount, [Optional] IGuildUser user)
        {
            (Context.User as IGuildUser).EnsureStaff();
            int count = 0;
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
            _ = Task.Run(async () => await messageToDel.TimedDeletion(3000));
        }
    }
}
