using System;
using System.Linq;
using System.Collections.Generic;

using Discord;

using Newtonsoft.Json;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store custom commands.
    /// </summary>
    public sealed class CustomizedCommandTable : LookupTable<string, string>
    {
        public CustomizedCommandTable(string path) : base(path)
        {
        }
    }
}