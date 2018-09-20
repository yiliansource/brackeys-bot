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
        /// Returns the total number of leaderboard users.
        /// </summary>
        public int TotalLeaderboardUsers => _lookup.Keys.Count;

        public KarmaTable(string path) : base(path)
        {
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
        /// Returns the leaderboard, sorted by points.
        /// </summary>
        public IEnumerable<KeyValuePair<ulong, int>> GetSortedLeaderboard ()
        {
            return _lookup
                .OrderByDescending(t => t.Value);
        }

        /// <summary>
        /// Returns the leaderboard, starting from the startIndex, including a specific number of places.
        /// </summary>
        public IEnumerable<KeyValuePair<ulong, int>> GetLeaderboardPlaces (int startIndex, int count)
        {
            var leaderboard = GetSortedLeaderboard();

            if (leaderboard.Count() < startIndex) return Enumerable.Empty<KeyValuePair<ulong, int>>();
            var skippedToIndex = leaderboard.Skip(startIndex);

            if (skippedToIndex.Count() < count) return skippedToIndex;

            return skippedToIndex.Take(count);
        }
    }
}