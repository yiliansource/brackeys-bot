using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BrackeysBot
{
    static class RegexUtils
    {
        private const string OrWithParentheses = @"\(([^()]*)\)\|\([^()]*\)";
        private const string OrWithNoParentheses = @"^(.*)\|.*$";
        private const string Parentheses = @"^([^\[]*)\((.*)\)";
        private const string NegatedCharacterSet = @"\[\^(.*)\]";
        private const string CharacterSet = @"\[.*(.)\]";
        private const string Optional = @"\\?[^\\]\?";
        private const string Literal = @"\\(.)";

        private const string RangeWithinSet = @"((\[[^\^].*?)|(\[))(.)\-(.)(.*?\])";
        private const string RangeWithinNegatedSet = @"(\[\^.*?)(.)\-(.)(.*?\])";

        public static string GetExample(string pattern) {
            // Replace all "(a)|(b)" to "a"
            while (Regex.IsMatch(pattern, OrWithParentheses))
                pattern = Regex.Replace(pattern, OrWithParentheses, @"$1");

            // Replace all "a|b" to "a"
            pattern = Regex.Replace(pattern, OrWithNoParentheses, @"$1");

            // Replaces characters classes
            pattern = pattern.Replace(@"\d", @"0");
            pattern = pattern.Replace(@"\w", @"a");
            pattern = pattern.Replace(@"\s", @" ");

            // Change all "[" to ACSII tags (needed for parentheses conversion)
            pattern = pattern.Replace(@"\[", $"<{(int)'['}>");

            // Replace all "(a)" to "a"
            while (Regex.IsMatch(pattern, Parentheses))
                pattern = Regex.Replace(pattern, Parentheses, @"$1");

            // Change all [ ACSII tags back to "\["
            pattern = pattern.Replace($"<{(int)'['}>", @"\[");

            // Replace all "[0-9]" to "[0]"
            while (Regex.IsMatch(pattern, RangeWithinSet))
                pattern = Regex.Replace(pattern, RangeWithinSet, @"$1$4$6");

            // Replace all "[^0-9]" to [^0123456789]
            while (Regex.IsMatch(pattern, RangeWithinNegatedSet))
                pattern = Regex.Replace(pattern, RangeWithinNegatedSet, RangeWithinNegatedSetEvaluator);

            pattern = Regex.Replace(pattern, @"<><(\d+)><>", m => $"{(char)int.Parse(m.Groups[1].Value)}");

            // Replace all "[^abc]" to "\d"
            pattern = Regex.Replace(pattern, NegatedCharacterSet, NegatedSetEvaluator);

            // Replace all "[abc]" to "c"
            pattern = Regex.Replace(pattern, CharacterSet, @"$1");

            // Replace all "ab?" to "a"
            pattern = Regex.Replace(pattern, Optional, @"");

            // Replace all "\?" to "?"
            pattern = Regex.Replace(pattern, Literal, @"$1");

            return pattern;
        }


        static string NegatedSetEvaluator(Match m) {
            MatchCollection allCharacters = Regex.Matches(m.Groups[1].Value, @"\\.|.");
            HashSet<int> negatedCharacter = new HashSet<int>();
            foreach (Match c in allCharacters) {
                negatedCharacter.Add(Regex.Match(c.Value, @"\\?(.)").Groups[1].Value[0]);
            }

            // Loop through all characters from ! to } in ASCII table
            for (int i = 33; i <= 125; i++) {
                if (!negatedCharacter.Contains(i))
                    return $@"\{(char)i}";
            }
            throw new Exception("Suitable character can't be found");
        }

        static string RangeWithinNegatedSetEvaluator(Match m) {
            StringBuilder sb = new StringBuilder();
            sb.Append(m.Groups[1].Value);

            for (int i = m.Groups[2].Value[0]; i <= m.Groups[3].Value[0]; i++) {
                sb.Append($"<><{i}><>");
            }

            sb.Append(m.Groups[4].Value);

            return sb.ToString();
        }
    }
}
