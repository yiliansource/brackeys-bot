using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store (temporary) user bans.
    /// </summary>
    public class BanTable : LookupTable<string, string>
    {
        public override string FileName => "bans";
        public override bool RequiresTemplateFile => false;

        public Dictionary<string, string> Bans => Table;
    }
}
