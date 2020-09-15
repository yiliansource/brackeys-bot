using System;

namespace BrackeysBot
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireHelperAttribute : RequirePermissionLevelAttribute
    {
        public RequireHelperAttribute() : base(PermissionLevel.Helper) { }
    }
}
