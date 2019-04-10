using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;

using BrackeysBot.Modules;

namespace BrackeysBot.Commands.Moderation
{
    public class ClearCommand : ModuleBase
    {
        private AuditLog _auditLog;

        public ClearCommand(AuditLog auditLog)
        {
            _auditLog = auditLog;
        }

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

            int clearedMessages = user == null ? amount : count;

            IMessage messageToDel = await ReplyAsync($":white_check_mark: Successfully cleared {clearedMessages} messages{((user != null) ? $" sent by {user.GetDisplayName()}" : string.Empty)}.");
            _ = messageToDel.TimedDeletion(3000);
            
            await _auditLog.AddEntry($"{(Context.User as IGuildUser).GetDisplayName()} cleared {clearedMessages} messages{((user != null) ? $" sent by {user.GetDisplayName()}" : string.Empty)} in <#{Context.Channel.Id}>.");
        }
    }
}
