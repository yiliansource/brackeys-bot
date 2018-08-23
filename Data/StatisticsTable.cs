using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store and change settings.
    /// </summary>
    public class StatisticsTable : LookupTable<string, uint>
    {
        public int TotalLeaderboardUsers => _lookup.Keys.Count;
        
        public StatisticsTable(string path) : base(path)
        {
        }

        /// <summary>
        /// Returns the leaderboard, sorted by points.
        /// </summary>
        public IEnumerable<KeyValuePair<string, uint>> GetSortedLeaderboard ()
        {
            return _lookup
                .OrderByDescending(t => t.Value);
        }

        /// <summary>
        /// Returns the leaderboard, starting from the startIndex, including a specific number of places.
        /// </summary>
        public IEnumerable<KeyValuePair<string, uint>> GetLeaderboardPlaces (int startIndex, int count)
        {
            var leaderboard = GetSortedLeaderboard();

            if (leaderboard.Count() < startIndex) return Enumerable.Empty<KeyValuePair<string, uint>>();
            var skippedToIndex = leaderboard.Skip(startIndex);

            if (skippedToIndex.Count() < count) return skippedToIndex;

            return skippedToIndex.Take(count);
        }
    }
}
