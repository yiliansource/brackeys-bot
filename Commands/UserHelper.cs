using System;
using System.Linq;

using Discord;

namespace BrackeysBot.Commands
{
    /// <summary>
    /// Provides extension methods for discord users.
    /// </summary>
    public static class UserHelper
    {
        /// <summary>
        /// Returns the displayed name for the specified user.
        /// </summary>
        public static string GetDisplayName(this IGuildUser user)
        {
            return string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
        }

        /// <summary>
        /// Ensures that the specified user has the "Staff" role. Throws an exception if he doesn't.
        /// </summary>
        public static void EnsureStaff(this IGuildUser user)
        {
            user.EnsureRole("Staff");
        }
        /// <summary>
        /// Ensures that the user has any of the given roles.
        /// </summary>
        public static void EnsureAnyRole (this IGuildUser user, params string[] roles)
        {
            if (!roles.Any(role => user.HasRole(role)))
            {
                throw new Exception("Insufficient permissions.");
            }
        }
        /// <summary>
        /// Ensures that the user has the specified role.
        /// </summary>
        public static void EnsureRole (this IGuildUser user, string role)
        {
            if (!user.HasRole(role))
            {
                throw new Exception("Insufficient permissions.");
            }
        }

        /// <summary>
        /// Checks if the user has the specified role.
        /// </summary>
        public static bool HasRole(this IGuildUser user, string role)
        {
            var staffRole = user.Guild.Roles.FirstOrDefault(r => string.Equals(r.Name, role, StringComparison.CurrentCultureIgnoreCase));
            return user.RoleIds.Any(id => id == staffRole?.Id);
        }
    }
}
