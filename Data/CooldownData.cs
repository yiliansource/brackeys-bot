using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace BrackeysBot
{
    public class CooldownData
    {
        public List<CommandCooldown<UserCooldown>> Commands { get; set; } = new List<CommandCooldown<UserCooldown>> ();
        public List<CommandCooldown<UserCooldownParameters>> SameParameterCommands { get; set; } = new List<CommandCooldown<UserCooldownParameters>> ();

        public static CooldownData FromPath(string path)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, "{}");

            return JsonConvert.DeserializeObject<CooldownData>(File.ReadAllText(path));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
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