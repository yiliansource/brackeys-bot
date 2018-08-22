using System;
using System.Collections.Generic;

namespace BrackeysBot.Data
{
    public class CooldownData
    {
        public List<CommandCooldown> Commands { get; set; } 
    }

    public class CommandCooldown
    {
        public string CommandName { get; set; }
        public ulong CooldownTime { get; set; }
        public CooldownType CooldownType { get; set; } = CooldownType.SameCommand;
        public List<UserCooldown> Users { get; set; }
    }

    public class UserCooldown
    {
        public ulong Id { get; set; }
        public string CommandExecutedTime { get; set; }
    }

    public enum CooldownType
    {
        SameParameters,
        SameCommand
    }
}