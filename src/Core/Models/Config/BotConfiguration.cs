﻿using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class BotConfiguration
    {
        #region General Configuration

        [YamlMember(Alias = "token"), Confidential]
        [Description("The token that is used to log in the bot.")]
        public string Token { get; set; }

        [YamlMember(Alias = "prefix")]
        [Description("The prefix that the bot commands will use.")]
        public string Prefix { get; set; } = "[]";

        #endregion

        #region IDs

        [YamlMember(Alias = "guildId")]
        [Description("The ID of the guild the bot is meant to act in.")]
        public ulong GuildID { get; set; }

        [YamlMember(Alias = "guruRoleId")]
        [Description("The ID of the role that identifies gurus.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong GuruRoleID { get; set; }

        [YamlMember(Alias = "moderatorRoleId")]
        [Description("The ID of the role that identifies moderators.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong ModeratorRoleID { get; set; }

        [YamlMember(Alias = "helperRoleId")]
        [Description("The ID of the role that identifies helpers.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong HelperRoleID { get; set; }

        [YamlMember(Alias = "mutedRoleId")]
        [Description("The ID of the role that mutes someone.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong MutedRoleID { get; set; }

        [YamlMember(Alias = "developerRoleId")]
        [Description("The ID of the bot developer role.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong DeveloperRoleID { get; set; }

        [YamlMember(Alias = "modLogChannelId")]
        [Description("The ID of the channel where moderation actions are logged.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong ModerationLogChannelID { get; set; }

        [YamlMember(Alias = "teamRoleIds")]
        [Description("A list of team role IDs.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong[] TeamRoleIDs { get; set; }

        [YamlMember(Alias = "userRoleIds")]
        [Description("A list of user role IDs.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong[] UserRoleIDs { get; set; }

        [YamlMember(Alias = "loggableIds")]
        [Description("A list of IDs that should be logged when pinged.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong[] LoggableIDs { get; set; }

        [YamlMember(Alias = "allowedCodeblockChannelIds")]
        [Description("A list of IDs where massive codeblocks are allowed.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong[] AllowedCodeblockChannelIDs { get; set; }

        [YamlMember(Alias = "amaChannelId")]
        [Description("The channel to extract ama questions from")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong AmaChannelID { get; set; }

        [YamlMember(Alias = "paidChannelId")]
        [Description("Channel to post paid collaboration embeds.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong PaidChannelId { get; set; }

        [YamlMember(Alias = "hobbyChannelId")]
        [Description("Channel to post hobby collaboration embeds.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong HobbyChannelId { get; set; }

        [YamlMember(Alias = "gametestChannelId")]
        [Description("Channel to post gametest collaboration embeds.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong GametestChannelId { get; set; }

        [YamlMember(Alias = "mentorChannelId")]
        [Description("Channel to post mentor collaboration embeds.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong MentorChannelId { get; set; }
        
        [YamlMember(Alias = "noCollabRoleId")]
        [Description("Role ID of the no collab role.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.RoleId)]
        public ulong NoCollabRoleId { get; set; }

        #endregion

        [YamlMember(Alias = "moduleConfigs")]
        [Description("The configurations of the modules the bot uses.")]
        public Dictionary<string, bool> ModuleConfigurations { get; set; } = new Dictionary<string, bool>();

        [YamlMember(Alias = "rules")]
        [Description("The rules that members in the servers should follow.")]
        [Shorten(50)]
        public string[] Rules { get; set; }

        [YamlMember(Alias = "codeblockThreshold")]
        [Description("The minimum length of a message to be seen as a codeblock.")]
        public int CodeblockThreshold { get; set; }

        [YamlMember(Alias = "blockedWords")]
        [Description("A list of regex for words which should automatically be deleted.")]
        public string[] BlockedWords { get; set; }
        
        [YamlMember(Alias = "blockedGuildIds")]
        [Description("A list of guild IDs for guild invites which should automatically be deleted.")]
        public ulong[] BlockedGuildIds { get; set; }

        [YamlMember(Alias = "clearMessageMaxHistory")]
        [Description("The maximum messages history count to fetch when clearing messages.")]
        public int ClearMessageMaxHistory { get; set; }

        [YamlMember(Alias = "infoCategoryId")]
        [Description("The ID of the category to display membercount.")]
        [ConfigDisplay(ConfigDisplayAttribute.Mode.ChannelId)]
        public ulong InfoCategoryId { get; set; }

        [YamlMember(Alias = "infoCategoryDisplay")]
        [Description("The display value of the info category. '%s%' will be replaced with membercount.")]
        public string InfoCategoryDisplay { get; set; }

        [YamlMember(Alias = "emoteRestrictions")]
        [Description("A list of channels and their emote restrictions")]
        public Dictionary<ulong, List<string>> EmoteRestrictions { get; set; }

        [YamlMember(Alias = "gamejamTimestamps")]
        [Description("The timestamps that outline a gamejam.")]
        public long[] GamejamTimestamps { get; set; }        
        
        [YamlMember(Alias = "endorseTimeoutMillis")]
        [Description("The minimum time between being able to endorse the same user again, in milliseconds")]
        public int EndorseTimeoutMillis { get; set; }
        
        [YamlMember(Alias = "latexTimeoutMillis")]
        [Description("The minimum time between being able to use the latex command again, in milliseconds")]
        public int LatexTimeoutMillis { get; set; }
 
        [YamlMember(Alias = "collabTimeoutMillis")]
        [Description("The minimum time between being able to use the collab command again, in milliseconds")]
        public int CollabTimeoutMillis { get; set; }

        [YamlMember(Alias = "codeFormatterDeleteTresholdMillis")]
        [Description("The maximum time before a message can be deleted when the format code command is used by a guru, in milliseconds")]
        public int CodeFormatterDeleteTresholdMillis { get; set; }

        [YamlMember(Alias = "helperMuteMaxDuration")]
        [Description("The maximum amount of time a helper may mute a person")]
        public int HelperMuteMaxDuration { get; set; }

        [YamlMember(Alias = "muteUserIfUsingFilteredWord")]
        [Description("Whether or not to mute people using the filtered word")]
        public bool MuteUserIfUsingFilteredWord { get; set; }

        [YamlMember(Alias = "filteredWordMuteDuration")]
        [Description("The Duration to mute a person for when using a filtered word")]
        public int FilteredWordMuteDuration { get; set; }
    }
}
