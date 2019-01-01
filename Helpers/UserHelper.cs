using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using BrackeysBot.Modules;

namespace BrackeysBot
{
    /// <summary>
    /// Provides extension methods for discord users.
    /// </summary>
    public static class UserHelper
    {
        public static DataModule Data { get; set; }

        /// <summary>
        /// Returns the displayed name for the specified user.
        /// </summary>
        public static string GetDisplayName(this IGuildUser user)
        {
            return string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
        }

        /// <summary>
        /// Ensures that the specified user is staff. Throws an exception if they aren't.
        /// </summary>
        public static void EnsureStaff(this IGuildUser user)
        {
            user.EnsureRole(Data.Settings["staff-role"]);
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

        /// <summary>
        /// Checks if the user is staff.
        /// </summary>
        public static bool HasStaffRole(this IGuildUser user)
        {
            return HasRole(user, Data.Settings["staff-role"]);
        }
        /// <summary>
        /// Checks if the user is a guru.
        /// </summary>
        public static bool HasGuruRole(this IGuildUser user)
        {
            return HasRole(user, Data.Settings["guru-role"]);
        }

        /// <summary>
        /// Mutes a user.
        /// </summary>

        public static async Task Mute (this IGuildUser user)
        {
            await user.AddRoleAsync(user.Guild.Roles.First(x => x.Name == "Muted"));
        }

        public static async Task Unmute (this IGuildUser user)
        {
            await user.RemoveRoleAsync(user.Guild.Roles.First(x => x.Name == "Muted"));
        }

        public static long GetMuteTime (this IGuildUser user)
        {
            return long.Parse(Data.Mutes.GetOrDefault(user.Id.ToString() + "," + user.Guild.Id.ToString()));
        }
    }
}
