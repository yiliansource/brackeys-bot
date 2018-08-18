using System;
using System.Linq;

using Discord;

namespace BrackeysBot.Commands
{
    /// <summary>
    /// Provides methods to verify Staff.
    /// </summary>
    public static class StaffCommandHelper
    {
        /// <summary>
        /// Ensures that the specified user has the "Staff" role. Throws an exception if he doesn't.
        /// </summary>
        public static void EnsureStaff (IGuildUser user)
        {
            if (!HasStaffRole(user))
            {
                throw new Exception("Insufficient permissions.");
            }
        }

        /// <summary>
        /// Checks if a specified user has the Staff role.
        /// </summary>
        public static bool HasStaffRole (IGuildUser user)
        {
            var staffRole = user.Guild.Roles.First(r => r.Name == "Staff");
            return user.RoleIds.Any(id => id == staffRole.Id);
        }
    }
}
