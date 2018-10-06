using System.Collections.Generic;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store user mutes.
    /// </summary>
    public class MuteTable : LookupTable<string, string>
    {
        public override string FileName => "mute";
        public override bool RequiresTemplateFile => false;

        public Dictionary<string, string> Mutes => Table;
    }
}
