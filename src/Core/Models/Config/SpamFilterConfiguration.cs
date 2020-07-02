using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class SpamFilterConfiguration
    {
        [YamlMember(Alias = "muteDuration")]
        [Description("The duration of the temporary mute in seconds (when the user triggers the spam conditions).")]
        public int MuteDuration { get; set; } = 30;

        [YamlMember(Alias = "consecutiveWordThreshold")]
        [Description("How many occurences of the same spam word in a row a message has to contain to trigger this spam condition")]
        public int ConsecutiveWordThreshold { get; set; } = 5;

        [YamlMember(Alias = "fullMessageWordThreshold")]
        [Description("How many occurences of the same spam word a message has to contain in total to trigger this spam condition")]
        public int FullMessageWordThreshold { get; set; } = 15;

        [YamlMember(Alias = "includeMentions")]
        [Description("Whether or not too many mentions of a combination of user/channel/role in one message are registered as spam.")]
        public bool IncludeMentions { get; set; } = false;

        [YamlMember(Alias = "mentionsThreshold")]
        [Description("How many mentions are required that a message is flagged as spam.")]
        public int MentionsThreshold { get; set; } = 8;

        [YamlMember(Alias = "includeEmotes")]
        [Description("Whether or not the use of too many emotes in one message. Custom emotes are always checked then, default Discord emotes only if enabled")]
        public bool IncludeEmotes { get; set; } = false;

        [YamlMember(Alias = "checkForDefaultEmotes")]
        [Description("If includeEmotes is active, will also consider Discord's default emotes in the check. Warning: enabling this option will be computationally expensive")]
        public bool CheckForDefaultEmotes { get; set; } = true;

        [YamlMember(Alias = "emotesThreshold")]
        [Description("How many emotes are required that a message is flagged as spam.")]
        public int EmotesThreshold { get; set; } = 8;
    }
}
