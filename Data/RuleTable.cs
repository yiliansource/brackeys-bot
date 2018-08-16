using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace BrackeysBot
{
    public class RuleTable : LookupTable<Dictionary<int, string>>
    {
        protected override Dictionary<int, string> Lookup { get; set; }

        public RuleTable()
        {
        }

        protected override string GetFilePath()
            => Path.Combine(Directory.GetCurrentDirectory(), "rules.json");

        public string this[int id]
        {
            get
            {
                string value = "";
                Lookup.TryGetValue(id, out value);
                return value;
            }
            set
            {
                if (!Lookup.TryAdd(id, value))
                    Lookup[id] = value;

                SaveData();
            }
        }

        public bool HasRule(int id)
            => Lookup.ContainsKey(id);
    }
}
