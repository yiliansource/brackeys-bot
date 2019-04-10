using System.Threading.Tasks;

using Discord;

namespace BrackeysBot.Modules
{
    /// <summary>
    /// Provides methods to append messages to the audit log channel.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// The channel that the audit log messages will be sent in.
        /// </summary>
        public IMessageChannel Channel { get; set; }

        /// <summary>
        /// Adds a text entry in the log channel.
        /// </summary>
        public async Task<IMessage> AddEntry(string message)
        {
            if (Channel == null) return null;

            return await Channel.SendMessageAsync(message);
        }
        /// <summary>
        /// Adds an embed entry in the log channel.
        /// </summary>
        public async Task<IMessage> AddEntry(Embed embed)
        {
            if (Channel == null) return null;

            return await Channel.SendMessageAsync(string.Empty, false, embed);
        }
    }
}
