using System;
using System.Linq;
using System.Collections.Generic;

using Discord;

using Newtonsoft.Json;

namespace BrackeysBot
{
    public sealed class KarmaTable : LookupTable<ulong, int>
    {
        /// <summary>
        /// Stores the cooldowns that a user has before he can thank another user again.
        /// </summary>
        [JsonIgnore]
        private Dictionary<ulong, Dictionary<ulong, DateTime>> _thankCooldowns { get; set; } = new Dictionary<ulong, Dictionary<ulong, DateTime>>();

        /// <summary>
        /// Stores the cooldown that a user has before he can use the []points user command again.
        /// </summary>
        [JsonIgnore]
        private Dictionary<ulong, DateTime> _pointsUserUsageCooldown { get; set; } = new Dictionary<ulong, DateTime>();

        /// <summary>
        /// Returns the total number of leaderboard users.
        /// </summary>
        public int TotalLeaderboardUsers => _lookup.Keys.Count;

        public KarmaTable(string path) : base(path)
        {
        }

        /// <summary>
        /// Performs the standard thank routine by adding one karma to the target and refreshing the cooldown.
        /// </summary>
        public void ThankUser (IUser source, IUser target, int cooldownSeconds)
        {
            AddKarma(target);
            AddThanksCooldown(source, target, cooldownSeconds);
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
        public void AddKarma (IUser user, int value)
        {
            ulong id = user.Id;
            if (Has(id))
            {
                SetKarma(user, this[id] + value);
            }
            else
            {
                Add(id, value);
            }
        }
        /// <summary>
        /// Removes a specific amount of karma from the specified user.
        /// </summary>
        public void RemoveKarma (IUser user, int value)
        {
            AddKarma(user, -value);
        }
        /// <summary>
        /// Sets the users karma to a specific amount.
        /// </summary>
        public void SetKarma (IUser user, int value)
        {
            ulong id = user.Id;
            if (Has(id)) Set(id, value);
            else Add(id, value);
        }
        
        /// <summary>
        /// Returns the karma for a specified user.
        /// </summary>
        public int GetKarma (IUser user)
        {
            ulong id = user.Id;
            if (Has(id)) return Get(id);
            else return 0;
        }

        /// <summary>
        /// Adds the thanks cooldown to the lookup.
        /// </summary>
        public void AddThanksCooldown (IUser source, IUser target, int cooldownSeconds)
        {
            if (cooldownSeconds <= 0) return;

            DateTime now = DateTime.Now;
            DateTime reEnable = now.AddSeconds(cooldownSeconds);

            ulong s = source.Id, t = target.Id;

            if (_thankCooldowns.ContainsKey(s))
            {
                if (_thankCooldowns[s].ContainsKey(t))
                    _thankCooldowns[s][t] = reEnable;
                else
                    _thankCooldowns[s].Add(t, reEnable);
            }
            else
            {
                _thankCooldowns.Add(s, new Dictionary<ulong, DateTime>() { { t, reEnable } });
            }
        }
        /// <summary>
        /// Checks if a cooldown for a thanks operation from source to target has already expired.
        /// </summary>
        public bool CheckThanksCooldownExpired (IUser source, IUser target, out int remainingMinutes)
        {
            ulong s = source.Id, t = target.Id;
            remainingMinutes = 0;

            if (!_thankCooldowns.ContainsKey(s)) return true;
            if (!_thankCooldowns[s].ContainsKey(t)) return true;

            DateTime limit = _thankCooldowns[s][t];
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
        public void CleanThanksCooldowns ()
        {
            var now = DateTime.Now;

            // Iterate over each element in the dictionary, checking if the limit time is still greater than the current time.
            // Remove elements that are already above the threshold
            foreach (ulong key in _thankCooldowns.Keys)
                _thankCooldowns[key] = _thankCooldowns[key].Where(k => k.Value <= now).ToDictionary(d => d.Key, d => d.Value);

            // Clean the base dictionary, keeping only values that still have valid dictionaries
            _thankCooldowns = _thankCooldowns.Where(k => k.Value.Keys.Count > 0).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Adds the points user cooldown to the lookup.
        /// </summary>
        public void AddPointsUserCooldown (IUser source, int cooldownSeconds)
        {
            if (cooldownSeconds <= 0) return;

            DateTime now = DateTime.Now;
            DateTime reEnable = now.AddSeconds(cooldownSeconds);

            ulong s = source.Id;
            if (_pointsUserUsageCooldown.ContainsKey(s))
            {
                _pointsUserUsageCooldown[s] = reEnable;
            }
            else
            {
                _pointsUserUsageCooldown.Add(s, reEnable);
            }
        }
        /// <summary>
        /// Checks if a cooldown for the command usage from the source user has already expired.
        /// </summary>
        public bool CheckPointsUserCooldownExpired (IUser source, out int remainingSeconds)
        {
            ulong s = source.Id;
            remainingSeconds = 0;

            if (!_pointsUserUsageCooldown.ContainsKey(s)) return true;

            DateTime limit = _pointsUserUsageCooldown[s];
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
        public void CleanPointsUserCooldowns ()
        {
            var now = DateTime.Now;

            // Clean the base dictionary, keeping only values that havent expired yet
            _pointsUserUsageCooldown = _pointsUserUsageCooldown.Where(k => k.Value <= now).ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Returns the leaderboard, sorted by points.
        /// </summary>
        public IEnumerable<Tuple<ulong, int>> GetSortedLeaderboard ()
        {
            return _lookup
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
