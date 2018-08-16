using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace BrackeysBot
{
    public class SettingsTable : LookupTable<Dictionary<string, string>>
    {
        protected override Dictionary<string, string> Lookup { get; set; }

        public SettingsTable()
        {
        }

        protected override string GetFilePath()
            => Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

        public string this[string id]
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
    }
}
