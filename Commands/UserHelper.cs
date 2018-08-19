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
            if (!user.HasStaffRole())
            {
                throw new Exception("Insufficient permissions.");
            }
        }

        /// <summary>
        /// Checks if a specified user has the Staff role.
        /// </summary>
        public static bool HasStaffRole(this IGuildUser user)
        {
            return user.HasRole("Staff");
        }

        /// <summary>
        /// Checks if the user has the specified role.
        /// </summary>
        public static bool HasRole (this IGuildUser user, string role)
        {
            var staffRole = user.Guild.Roles.First(r => r.Name == role);
            return user.RoleIds.Any(id => id == staffRole.Id);
        }
    }
}
