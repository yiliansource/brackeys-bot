using Discord;

namespace BrackeysBot.Commands
{
    public static class UserHelper
    {
        /// <summary>
        /// Returns the displayed name for the specified user.
        /// </summary>
        public static string GetDisplayName(IGuildUser user)
        {
            return string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
        }
    }
}
