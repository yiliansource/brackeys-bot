using Discord;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BrackeysBot.Commands
{
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
        public static bool HasStaffRole (IGuildUser user)
        {
            var staffRole = user.Guild.Roles.First(r => r.Name == "Staff");
            return user.RoleIds.Any(id => id == staffRole.Id);
        }
    }
}
