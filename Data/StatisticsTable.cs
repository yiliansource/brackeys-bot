using System.Collections.Generic;
using System.Linq;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store statistics.
    /// </summary>
    public class StatisticsTable : LookupTable<string, uint>
    {
        public override string FileName => "statistics";

        /// <summary>
        /// Returns the statistics, sorted by points.
        /// </summary>
        public IEnumerable<KeyValuePair<string, uint>> GetSortedStatistics ()
        {
            return Table
                .OrderByDescending(t => t.Value);
        }
    }
}
