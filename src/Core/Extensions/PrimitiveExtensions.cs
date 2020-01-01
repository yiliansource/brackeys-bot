using System;
using System.Collections.Generic;
using System.Text;

namespace BrackeysBot
{
    public static class PrimitiveExtensions
    {
        public static string AsNullIfEmpty(this string s)
            => string.IsNullOrEmpty(s) ? null : s;
        public static string WithAlternative(this string s, string alternative)
            => s.AsNullIfEmpty() ?? alternative;
        public static string Envelop(this string s, string outer)
            => new StringBuilder().Append(outer).Append(s).Append(outer).ToString();
        public static string Envelop(this string s, string prefix, string postfix)
            => new StringBuilder().Append(prefix).Append(s).Append(postfix).ToString();
    }
}
