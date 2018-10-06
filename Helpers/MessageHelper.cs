using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot
{
    /// <summary>
    /// Provides extension methods for Discord messages.
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Deletes the specified message after the specified time period.
        /// </summary>
        public static async Task TimedDeletion(this IMessage message, int milliseconds)
        {
            await Task.Delay(milliseconds);
            await message.DeleteAsync();
        }
    }
}
