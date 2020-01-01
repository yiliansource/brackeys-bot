using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class BotConfiguration
    {
        [YamlMember(Alias = "token")]
        public string Token { get; set; }
        [YamlMember(Alias = "prefix")]
        public string Prefix { get; set; } = "[]";

        [YamlMember(Alias = "guild")]
        public ulong Guild { get; set; } = 0;
        [YamlMember(Alias = "modLogChannel")]
        public ulong ModerationLogChannel { get; set; } = 0;

        [YamlMember(Alias = "moduleConfigs")]
        public Dictionary<string, bool> ModuleConfigurations { get; set; } = new Dictionary<string, bool>();
    }
}
