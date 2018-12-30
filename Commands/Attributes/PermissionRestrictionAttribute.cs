using System;

namespace BrackeysBot
{
    /// <summary>
    /// Allows the restriction of a command to a certain role.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class PermissionRestrictionAttribute : Attribute
    {
        /// <summary>
        /// The roles that are allowed to invoke a command.
        /// </summary>
        public UserType AllowedRoles => _allowedRoles;
        private readonly UserType _allowedRoles;
        
        public PermissionRestrictionAttribute(UserType allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }
    }
}