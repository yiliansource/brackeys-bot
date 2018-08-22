using System.IO;
using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store the rules for a server.
    /// </summary>
    public class RuleTable : LookupTable<int, string>
    {
        /// <summary>
        /// Stores the cooldown that a user has before he can use the []points user command again.
        /// </summary>
        [JsonIgnore]
        private Dictionary<ulong, DateTime> _rulesUserUsageCooldown { get; set; } = new Dictionary<ulong, DateTime>();

        /// <summary>
        /// Returns the rules from the table.
        /// </summary>
        public Dictionary<int, string> Rules => _lookup;

        public RuleTable(string path) : base(path)
        {
        }

        /// <summary>
        /// Adds the points user cooldown to the lookup.
        /// </summary>
        public void AddRulesUserCooldown(IUser source, int cooldownSeconds)
        {
            if (cooldownSeconds <= 0) return;

            DateTime now = DateTime.Now;
            DateTime reEnable = now.AddSeconds(cooldownSeconds);

            ulong s = source.Id;
            if (_rulesUserUsageCooldown.ContainsKey(s))
            {
                _rulesUserUsageCooldown[s] = reEnable;
            }
            else
            {
                _rulesUserUsageCooldown.Add(s, reEnable);
            }
        }
        /// <summary>
        /// Checks if a cooldown for the command usage from the source user has already expired.
        /// </summary>
        public bool CheckRulesUserCooldownExpired(IUser source, out int remainingSeconds)
        {
            ulong s = source.Id;
            remainingSeconds = 0;

            if (!_rulesUserUsageCooldown.ContainsKey(s)) return true;

            DateTime limit = _rulesUserUsageCooldown[s];
            DateTime now = DateTime.Now;
            if (limit <= now)
            {
                return true;
            }
            else
            {
                remainingSeconds = (int)Math.Round((limit - now).TotalSeconds);
                return false;
            }
        }
        /// <summary>
        /// Cleans all points user cooldowns that are no longer needed.
        /// </summary>
        public void CleanRulesUserCooldowns()
        {
            var now = DateTime.Now;

            // Clean the base dictionary, keeping only values that havent expired yet
            _rulesUserUsageCooldown = _rulesUserUsageCooldown.Where(k => k.Value <= now).ToDictionary(d => d.Key, d => d.Value);
        }
    }
}
