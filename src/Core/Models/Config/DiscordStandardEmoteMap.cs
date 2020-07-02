using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using System.Linq;

namespace BrackeysBot
{

    public class DiscordStandardEmoteMap
    {
        [YamlMember(Alias = "emoteMap")]
        [Description("A lookup table with the emote name as key and the .")]
        public Dictionary<string, string> EmoteMap { get; set; }

        public string[] EmoteValues => EmoteMap != null ? EmoteMap.Select(x => x.Value).ToArray() : new string[0];
    }
}
