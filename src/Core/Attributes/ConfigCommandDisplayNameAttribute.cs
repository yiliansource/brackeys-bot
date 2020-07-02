using System;
using System.Collections.Generic;
using System.Text;

namespace BrackeysBot
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigCommandDisplayNameAttribute : Attribute
    {
        public string DisplayName => displayName;
        private readonly string displayName;

        public ConfigCommandDisplayNameAttribute(string _displayName)
        {
            displayName = _displayName;
        }
    }
}
