using System;

namespace BrackeysBot
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class HelpDataAttribute : Attribute
    {
        private readonly string _usage;
        private readonly string _description;

        public string Usage { get => _usage; }
        public string Description { get => _description; }
        public UserType AllowedRoles { get; set; } = UserType.Everyone;
        public int ListOrder { get; set; }

        public HelpDataAttribute(string usage, string description)
        {
            _usage = usage;
            _description = description;
        }
    }
}