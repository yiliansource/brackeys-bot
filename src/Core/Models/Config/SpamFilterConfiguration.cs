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
        [Description("The duration of the temporary mute in seconds (when the user triggers the spam conditions).")]
        public int ConsecutiveWordThreshold { get; set; } = 5;

        [YamlMember(Alias = "fullMessageWordThreshold")]
        [Description("The duration of the temporary mute in seconds (when the user triggers the spam conditions).")]
        public int FullMessageWordThreshold { get; set; } = 15;
    }
}
