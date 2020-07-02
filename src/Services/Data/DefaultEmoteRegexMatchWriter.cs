using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BrackeysBot
{
    public class DefaultEmoteRegexMatchWriter
    {
        /// <summary>
        /// Creates a file that contains the c# dictionary definition for the discord default emote map with information about 
        /// how many matches for each emote are found when checked against the regex pattern of all default emotes. This is to 
        /// counteract false positives by using (foundEmoteMatchesForEmoteType / regexMatchCountForSingleEmote)
        /// </summary>
        /// <param name="filePath">path at which the resulting file is stored</param>
        public static void WriteEmoteValueSet(string filePath)
        {
            string[] emoteValuesRegexEscaped = DiscordDefaultEmoteData.EmoteMap.Select(x => Regex.Escape(x.Value)).ToArray();
            // Double curly brackets to escape { as a special character
            string lineTemplate = "    {{ \"{0}\", new " + nameof(EmoteValueSet) + "(\"{1}\", {2}) }},";
            List<string> lines = new List<string>();
            foreach (var keyVal in DiscordDefaultEmoteData.EmoteMap)
            {
                string emoteTextRepresentation = keyVal.Value;
                int sumOfMatchedRegexEntries = emoteValuesRegexEscaped.Aggregate(0, (count, emoteTextRegexEscaped) => count + Regex.Matches(emoteTextRepresentation, emoteTextRegexEscaped).Count);
                lines.Add(string.Format(lineTemplate, keyVal.Key, emoteTextRepresentation, sumOfMatchedRegexEntries));
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public static Dictionary<string, {nameof(EmoteValueSet)}> EmoteMatchMap {{ get; private set; }} = new Dictionary<string, {nameof(EmoteValueSet)}>() {{");
            sb.Append(string.Join(Environment.NewLine, lines));
            sb.AppendLine("};");

            System.IO.File.WriteAllText(filePath, sb.ToString());
        }
    }
}
