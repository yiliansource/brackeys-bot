using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store and change settings.
    /// </summary>
    public class SettingsTable : LookupTable<string, string>
    {
        public override string FileName => "settings";
        public override bool RequiresTemplateFile => true;

        public Dictionary<string, string> Settings => Table;
    }
}
