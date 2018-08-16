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
            var staff = user.Guild.Roles.First(r => r.Name == "Staff");
            if(!user.RoleIds.Any(id => id == staff.Id))
            {
                throw new Exception("Insufficient permissions.");
            }
        }
    }
}
