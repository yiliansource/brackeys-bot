using System;

namespace BrackeysBot
{
    /// <summary>
    /// Allows to store data that can be listed via the []help command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class HelpDataAttribute : Attribute
    {
        private readonly string _usage;
        private readonly string _description;

        /// <summary>
        /// The usage of the command.
        /// </summary>
        public string Usage { get => _usage; }
        /// <summary>
        /// The description of the command.
        /// </summary>
        public string Description { get => _description; }

        /// <summary>
        /// The order that the command will be listed in.
        /// </summary>
        public int ListOrder { get; set; }

        public HelpDataAttribute(string usage, string description)
        {
            _usage = usage;
            _description = description;
        }
    }
}