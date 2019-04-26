using System;
using System.Collections.Generic;
using System.Text;

namespace BrackeysBot
{
    /// <summary>
    /// Utility to handle conversion from text with special characters to plain text and vice versa
    /// </summary>
    static class CommandConversion
    {
        const string SlashID = @"<92>";

        /// <summary>
        /// Converts from text with special characters to plain text
        /// </summary>
        public static string ToConverted(string source) => source.Replace(@"\", SlashID);
        /// <summary>
        /// Converts from plain text to text with special characters
        /// </summary>
        public static string FromConverted(string source) => source.Replace(SlashID, @"\");
    }
}
