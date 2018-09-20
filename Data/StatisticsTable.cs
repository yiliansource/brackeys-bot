using System.Collections.Generic;
using System.Linq;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store statistics.
    /// </summary>
    public class StatisticsTable : LookupTable<string, uint>
    {
        public StatisticsTable(string path) : base(path)
        {
        }

        /// <summary>
        /// Returns the leaderboard, sorted by points.
        /// </summary>
        public IEnumerable<KeyValuePair<string, uint>> GetSortedStatistics ()
        {
            return _lookup
                .OrderByDescending(t => t.Value);
        }
    }
}
