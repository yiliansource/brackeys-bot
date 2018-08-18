using System.IO;
using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store and change settings.
    /// </summary>
    public class SettingsTable : LookupTable<string, string>
    {
        public SettingsTable(string path) : base(path)
        {
        }
    }
}
