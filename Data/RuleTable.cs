using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store the rules for a server.
    /// </summary>
    public class RuleTable : LookupTable<int, string>
    {
        public override string FileName => "rules";
        public override bool RequiresTemplateFile => true;

        /// <summary>
        /// Returns the rules from the table.
        /// </summary>
        public Dictionary<int, string> Rules => _lookup;
    }
}
