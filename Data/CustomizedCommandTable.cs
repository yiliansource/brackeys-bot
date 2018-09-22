using System.Linq;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store custom commands.
    /// </summary>
    public sealed class CustomizedCommandTable : LookupTable<string, string>
    {
        public override string FileName => "custom-commands";

        /// <summary>
        /// Returns the names of all registered commands.
        /// </summary>
        public string[] CommandNames
            => _lookup.Keys.ToArray();
    }
}