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
        public Dictionary<int, string> Rules => Lookup;

        public RuleTable()
        {
        }

        protected override string GetFilePath()
            => Path.Combine(Directory.GetCurrentDirectory(), "rules.json");

        /// <summary>
        /// Returns a rule by its id.
        /// </summary>
        public string this[int id]
        {
            get
            {
                return Lookup[id];
            }
            set
            {
                Lookup[id] = value;
                SaveData();
            }
        }

        /// <summary>
        /// Checks if the rule table contains a rule with the specified id.
        /// </summary>
        public bool HasRule(int id)
            => Lookup.ContainsKey(id);

        /// <summary>
        /// Adds a rule to the table.
        /// </summary>
        public void AddRule (int id, string content)
        {
            Lookup.Add(id, content);
            SaveData();
        }

        /// <summary>
        /// Deletes the rule with the specified id.
        /// </summary>
        public void DeleteRule(int id)
        { 
            Lookup.Remove(id);
            SaveData();
        }
    }
}
