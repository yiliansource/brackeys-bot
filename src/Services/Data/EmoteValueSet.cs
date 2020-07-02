using System.Text.RegularExpressions;

namespace BrackeysBot
{
    public class EmoteValueSet
    {
        public string Value { get; private set; }
        public string ValueRegexEscaped { get; private set; }
        public int RegexMatchCount { get; private set; }

        public EmoteValueSet(string value, int regexMatchCount)
        {
            Value = value;
            ValueRegexEscaped = Regex.Escape(value);
            RegexMatchCount = regexMatchCount;
        }
    }
}
