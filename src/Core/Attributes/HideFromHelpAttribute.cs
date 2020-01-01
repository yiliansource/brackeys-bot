using System;

namespace BrackeysBot
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HideFromHelpAttribute : Attribute
    {
    }
}
