using System.Collections.Generic;

namespace BrackeysBot.Data
{
    public class CooldownData
    {
        public List<CommandCooldown<UserCooldown>> Commands { get; set; } = new List<CommandCooldown<UserCooldown>> ();
        public List<CommandCooldown<UserCooldownParameters>> SameParameterCommands { get; set; } = new List<CommandCooldown<UserCooldownParameters>> ();
    }

    public class CommandCooldown<T> where T : UserCooldown
    {
        public string CommandName { get; set; }
        public ulong CooldownTime { get; set; }
        public List<T> Users { get; set; } = new List<T> ();
    }

    public class UserCooldown
    {
        public ulong Id { get; set; }
        public string CommandExecutedTime { get; set; }
    }

    public class UserCooldownParameters : UserCooldown
    {
        public string Parameters { get; set; }
    }
}