using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.WebSocket;

namespace BrackeysBot
{
    public static class MessageExtensions
    {
        public static async Task SendToChannel(this Embed e, IMessageChannel channel)
            => await channel.SendMessageAsync(string.Empty, false, e);
        public static EmbedBuilder AddFieldConditional(this EmbedBuilder eb, bool condition, string name, object value, bool inline = false)
        {
            if (condition) {
                if (value is string) {
                    eb.AddField(name, cropToLength(value as string, 1024), inline);
                } else {
                    eb.AddField(name, value, inline);
                }
            }
            return eb;
        }

        public static async Task<bool> TrySendMessageAsync(this IUser user, string text = null, bool isTTS = false, Embed embed = null)
        {
            try
            {
                await user.SendMessageAsync(cropToLength(text, 2000), isTTS, embed);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string cropToLength(string msg, int length) {
            if (msg?.Length > length) 
                return msg.Substring(0, length - 3) + "...";
            return msg;
        }
    }
}
