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
        
        public static Dictionary<ulong, Dictionary<ulong, DateTime>> ThankCooldowns { get; set; } = new Dictionary<ulong, Dictionary<ulong, DateTime>>();
        public static Dictionary<ulong, DateTime> PointsUserUsageCooldown { get; set; } = new Dictionary<ulong, DateTime>();

        public int TotalLeaderboardUsers => Lookup.Keys.Count;

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
        /// Removes a specific amount of karma from the specified user.
        /// </summary>
        public void RemoveKarma (IUser user, int count)
        {
            AddKarma(user, -count);
        }
        /// <summary>
        /// Sets the users karma to a specific amount.
        /// </summary>
        public void SetKarma (IUser user, int amount)
        {
            ulong id = user.Id;
            if (Lookup.ContainsKey(id))
            {
                Lookup[id] = amount;
            }
            else
            {
                Lookup.Add(id, amount);
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
        public static void AddThanksCooldown (IUser source, IUser target)
        {
            int.TryParse(BrackeysBot.Settings["thanks"], out int cooldownSeconds);
            if (cooldownSeconds <= 0) return;

            DateTime now = DateTime.Now;
            DateTime reEnable = now.AddSeconds(cooldownSeconds);

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
        public static bool CheckThanksCooldownExpired (IUser source, IUser target, out int remainingMinutes)
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
        /// Cleans all thanks cooldowns that are no longer needed.
        /// </summary>
        public static void CleanThanksCooldowns ()
        {
            var now = DateTime.Now;

            // Iterate over each element in the dictionary, checking if the limit time is still greater than the current time.
            // Remove elements that are already above the threshold
            foreach (ulong key in ThankCooldowns.Keys)
                ThankCooldowns[key] = ThankCooldowns[key].Where(k => k.Value <= now).ToDictionary(d => d.Key, d => d.Value);

            // Clean the base dictionary, keeping only values that still have valid dictionaries
            ThankCooldowns = ThankCooldowns.Where(k => k.Value.Keys.Count > 0).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Adds the points user cooldown to the lookup.
        /// </summary>
        public static void AddPointsUserCooldown (IUser source)
        {
            int.TryParse(BrackeysBot.Settings["points-user"], out int cooldownSeconds);
            if (cooldownSeconds <= 0) return;

            DateTime now = DateTime.Now;
            DateTime reEnable = now.AddSeconds(cooldownSeconds);

            ulong s = source.Id;
            if (PointsUserUsageCooldown.ContainsKey(s))
            {
                PointsUserUsageCooldown[s] = reEnable;
            }
            else
            {
                PointsUserUsageCooldown.Add(s, reEnable);
            }
        }
        /// <summary>
        /// Checks if a cooldown for the command usage from the source user has already expired.
        /// </summary>
        public static bool CheckPointsUserCooldownExpired (IUser source, out int remainingSeconds)
        {
            ulong s = source.Id;
            remainingSeconds = 0;

            if (!PointsUserUsageCooldown.ContainsKey(s)) return true;

            DateTime limit = PointsUserUsageCooldown[s];
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
        public static void CleanPointsUserCooldowns ()
        {
            var now = DateTime.Now;

            // Clean the base dictionary, keeping only values that havent expired yet
            PointsUserUsageCooldown = PointsUserUsageCooldown.Where(k => k.Value <= now).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Returns the leaderboard, sorted by points.
        /// </summary>
        public IEnumerable<Tuple<ulong, int>> GetSortedLeaderboard ()
        {
            return Lookup
                .Select(k => new Tuple<ulong, int>(k.Key, k.Value))
                .OrderByDescending(t => t.Item2);
        }

        /// <summary>
        /// Returns the leaderboard, starting from the startIndex, including a specific number of places.
        /// </summary>
        public IEnumerable<Tuple<ulong, int>> GetLeaderboardPlaces (int startIndex, int count)
        {
            var leaderboard = GetSortedLeaderboard();

            if (leaderboard.Count() < startIndex) return Enumerable.Empty<Tuple<ulong, int>>();
            var skippedToIndex = leaderboard.Skip(startIndex);

            if (skippedToIndex.Count() < count) return skippedToIndex;

            return skippedToIndex.Take(count);
        }
    }
}
