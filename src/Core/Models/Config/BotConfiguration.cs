using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class BotConfiguration
    {
        [YamlMember(Alias = "token"), Confidential]
        [Description("The token that is used to log in the bot.")]
        public string Token { get; set; }

        [YamlMember(Alias = "prefix")]
        [Description("The prefix that the bot commands will use.")]
        public string Prefix { get; set; } = "[]";

        [YamlMember(Alias = "guildId")]
        [Description("The ID of the guild the bot is meant to act in.")]
        public ulong GuildID { get; set; }

        [YamlMember(Alias = "guruRoleId")]
        [Description("The ID of the role that identifies gurus.")]
        public ulong GuruRoleID { get; set; }

        [YamlMember(Alias = "moderatorRoleId")]
        [Description("The ID of the role that identifies moderators.")]
        public ulong ModeratorRoleID { get; set; }

        [YamlMember(Alias = "mutedRoleId")]
        [Description("The ID of the role that mutes someone.")]
        public ulong MutedRoleID { get; set; }

        [YamlMember(Alias = "modLogChannelId")]
        [Description("The ID of the channel where moderation actions are logged.")]
        public ulong ModerationLogChannelID { get; set; }

        [YamlMember(Alias = "moduleConfigs")]
        [Description("The configurations of the modules the bot uses.")]
        public Dictionary<string, bool> ModuleConfigurations { get; set; } = new Dictionary<string, bool>();
    }
}
