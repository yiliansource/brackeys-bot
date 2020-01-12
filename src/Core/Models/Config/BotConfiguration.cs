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

        [YamlMember(Alias = "guildId")]
        public ulong GuildID { get; set; }
        [YamlMember(Alias = "guruRoleId")]
        public ulong GuruRoleID { get; set; }
        [YamlMember(Alias = "moderatorRoleId")]
        public ulong ModeratorRoleID { get; set; }
        [YamlMember(Alias = "mutedRoleId")]
        public ulong MutedRoleID { get; set; }
        [YamlMember(Alias = "modLogChannelId")]
        public ulong ModerationLogChannelID { get; set; }

        [YamlMember(Alias = "moduleConfigs")]
        public Dictionary<string, bool> ModuleConfigurations { get; set; } = new Dictionary<string, bool>();
    }
}
