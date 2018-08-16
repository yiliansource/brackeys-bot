using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace BrackeysBot
{
    public class HelpTable : LookupTable<Dictionary<string, List<string>>>
    {
        [JsonProperty("Help")]
        protected override Dictionary<string, List<string>> Lookup { get; set; }

        public HelpTable()
        {
        }

        protected override string GetFilePath()
            => Path.Combine(Directory.GetCurrentDirectory(), "help.json");
        
        /// <summary>
        /// Gets the commands for a specific mode. Either "default" or "mod".
        /// </summary>
        public List<KeyValuePair<string, string>> GetCommands(string mode)
        {
            if (!Lookup.ContainsKey(mode))
                return new List<KeyValuePair<string, string>>();

            List<string> rawCommands = Lookup[mode];
            return rawCommands.Select(c => { string[] parts = c.Split('='); return new KeyValuePair<string, string>(parts[0], parts[1]); }).ToList();
        }
    }
}
