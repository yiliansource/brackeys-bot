using System;
using System.Collections.Generic;
namespace BrackeysBot
{
    [Serializable]
    public class WarningData
    {
        public long time;
        public int severity;
        public string warner;
        public string reason;
    }
    /// <summary>
    /// Provides a table to store warnings.
    /// </summary>
    public class WarningTable : LookupTable<string, List<WarningData>>
    {
        public override string FileName => "warns";
        public override bool RequiresTemplateFile => false;
        public Dictionary<string, List<WarningData>> Warns => Table;
    }
}