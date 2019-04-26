using System;
using System.Collections.Generic;
using System.Text;

namespace BrackeysBot.Helpers
{
    static class CommandConversion
    {
        const string SlashID = @"<87126>";

        public static string ToConverted(string source) => source.Replace(@"\", SlashID);
        public static string FromConverted(string source) => source.Replace(SlashID, @"\");
    }
}
