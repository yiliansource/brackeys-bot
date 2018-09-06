using System.IO;
using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store and change settings.
    /// </summary>
    public class GiveawayTable : LookupTable<string, string>
    {
        public GiveawayTable(string path) : base(path)
        {
        }
    }
}
