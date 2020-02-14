using Discord;

namespace BrackeysBot
{
    public static class ChannelExtensions
    {
        public static string Linkable(this IMessageChannel channel)
        {
            return $"<#{channel.Id}>";
        }
    }
}
