using System;

namespace BrackeysBot
{
    /// <summary>
    /// The type of a user.
    /// </summary>
    [Flags]
    public enum UserType
    {
        /// <summary>
        /// Not a user at all.
        /// </summary>
        None = 0,
        /// <summary>
        /// Everyone.
        /// </summary>
        Everyone = 1,
        /// <summary>
        /// The user is a guru.
        /// </summary>
        Guru = 2,
        /// <summary>
        /// The user is a staff member (highest).
        /// </summary>
        Staff = 4,

        /// <summary>
        /// The user is a staff member or a guru.
        /// </summary>
        StaffGuru = Staff | Guru
    }
}