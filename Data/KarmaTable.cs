using Discord;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BrackeysBot
{
    public sealed class KarmaTable : LookupTable<Dictionary<ulong, int>>
    {
        [JsonProperty("Karma")]
        protected override Dictionary<ulong, int> Lookup { get; set; }

        [JsonIgnore]
        public Dictionary<ulong, Dictionary<ulong, DateTime>> ThankCooldowns { get; set; } = new Dictionary<ulong, Dictionary<ulong, DateTime>>();

        public KarmaTable ()
        {
        }

        protected override string GetFilePath()
            => Path.Combine(Directory.GetCurrentDirectory(), "karmatable.json");

        /// <summary>
        /// Performs the standard thank routine by adding one karma to the target and refreshing the cooldown.
        /// </summary>
        public void ThankUser (IUser source, IUser target)
        {
            AddKarma(target);
            AddThanksCooldown(source, target);
        }

        /// <summary>
        /// Adds one karma to the specified user.
        /// </summary>
        public void AddKarma (IUser user)
        {
            AddKarma(user, 1);
        }
        /// <summary>
        /// Adds a specific amount of karma to the specified user.
        /// </summary>
        public void AddKarma (IUser user, int count)
        {
            ulong id = user.Id;
            if (Lookup.ContainsKey(id))
            {
                Lookup[id] += count;
            }
            else
            {
                Lookup.Add(id, count);
            }

            SaveData();
        }
        
        /// <summary>
        /// Returns the karma for a specified user.
        /// </summary>
        public int GetKarma (IUser user)
        {
            int karma = 0;
            Lookup.TryGetValue(user.Id, out karma);
            return karma;
        }

        /// <summary>
        /// Adds the thanks cooldown to the lookup.
        /// </summary>
        public void AddThanksCooldown (IUser source, IUser target)
        {
            int.TryParse(BrackeysBot.Settings["thanks"], out int cooldownMinutes);
            if (cooldownMinutes <= 0) return;

            DateTime now = DateTime.Now;
            DateTime reEnable = now.AddMinutes(cooldownMinutes);

            ulong s = source.Id, t = target.Id;

            if (ThankCooldowns.ContainsKey(s))
            {
                if (ThankCooldowns[s].ContainsKey(t))
                    ThankCooldowns[s][t] = reEnable;
                else
                    ThankCooldowns[s].Add(t, reEnable);
            }
            else
            {
                ThankCooldowns.Add(s, new Dictionary<ulong, DateTime>() { { t, reEnable } });
            }
        }

        /// <summary>
        /// Checks if a cooldown for a thanks operation from source to target has already expired.
        /// </summary>
        public bool CheckThanksCooldownExpired (IUser source, IUser target, out int remainingMinutes)
        {
            ulong s = source.Id, t = target.Id;
            remainingMinutes = 0;

            if (!ThankCooldowns.ContainsKey(s)) return true;
            if (!ThankCooldowns[s].ContainsKey(t)) return true;

            DateTime limit = ThankCooldowns[s][t];
            DateTime now = DateTime.Now;
            if (limit <= now)
            {
                return true;
            }
            else
            {
                remainingMinutes = (int)Math.Round((limit - now).TotalMinutes);
                return false;
            }
        }

        /// <summary>
        /// Cleans all thanks cooldowns that are not longer needed.
        /// </summary>
        public void CleanThanksCooldowns ()
        {
            // Iterate over each element in the dictionary, checking if the limit time is still greater than the current time.
            // Remove elements that are already above the threshold
            foreach (ulong key in ThankCooldowns.Keys)
                ThankCooldowns[key] = ThankCooldowns[key].Where(k => k.Value <= DateTime.Now).ToDictionary(d => d.Key, d => d.Value);

            // Clean the base dictionary, keeping only values that still have valid dictionaries
            ThankCooldowns = ThankCooldowns.Where(k => k.Value.Keys.Count > 0).ToDictionary(d => d.Key, d => d.Value);
        }
    }
}
