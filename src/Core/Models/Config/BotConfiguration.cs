using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class BotConfiguration
    {
        [YamlMember(Alias = "token")]
        public string Token { get; set; }
        [YamlMember(Alias = "prefix")]
        public string Prefix { get; set; } = "[]";
        [YamlMember(Alias = "modLogChannel")]
        public ulong ModerationLogChannel { get; set; } = 0;
        [YamlMember(Alias = "moduleConfigs")]
        public Dictionary<string, bool> ModuleConfigurations { get; set; } = new Dictionary<string, bool>();
    }
}
