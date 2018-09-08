using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.Commands;
using System.IO;

using Newtonsoft.Json;
using System.Text;
using BrackeysBot.Data;

namespace BrackeysBot.Commands
{
    [Serializable]
    public struct PasteRequest
    {
        [JsonProperty("encrypted")]
        public bool Encrypted { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("sections")]
        public Section[] Sections { get; set; }

        [Serializable]
        public struct Section
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("syntax")]
            public string Syntax { get; set; }
            [JsonProperty("contents")]
            public string Contents { get; set; }
        }
    }

    [Serializable]
    public struct PasteResponse
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
    }

    public class PasteCommand : ModuleBase
    {
        private const string PASTE_EE_URL = "https://api.paste.ee/v1/pastes";
        private const string PASTE_EE_API_KEY = "athpICq32L0e7FByPACnfniTkOX2aGgLROHwd10v9";
        private const int MASSIVE_THRESHOLD = 300;
        private const string CODEBLOCK_IDENTIFIER = "```";
        private static readonly Regex _codeblockRegex = new Regex($@"(?:{ CODEBLOCK_IDENTIFIER })(\w+)?\n([^{ CODEBLOCK_IDENTIFIER[0] }]*)", RegexOptions.Compiled);

        [Command("modpaste")]
        [HelpData("modpaste <message_id>", "Paste a specific message.", AllowedRoles = UserType.Staff)]
        [Alias("modhaste")]
        public async Task ModPasteMessage(ulong messageId)
        {
            var message = await Context.Channel.GetMessageAsync(messageId);
            var messageContent = message.Content;
            RemoveCodeblockFormat(ref messageContent, out string syntax);
            string url = await PasteMessage(messageContent, syntax);

            await ReplyAsync($"Message by { ((IGuildUser) message.Author).GetDisplayName() } was pasted to { url }.");
            await message.DeleteAsync();
        }

        [Command("paste")]
        [HelpData("paste <message>", "Pastes a message to paste.ee")]
        [Alias("haste")]
        public async Task PasteMessage([Remainder] string messageContent)
        {
            string content = messageContent.Trim('\n', ' ');
            RemoveCodeblockFormat(ref content, out string syntax);
            string url = await PasteMessage(content, syntax);

            await ReplyAsync($"{ (Context.User as IGuildUser).GetDisplayName() }, I created a paste for you! <{ url }>");
            await Context.Message.DeleteAsync();
        }

        /// <summary>
        /// Hastes a string and returns the URL of the hastebin page.
        /// </summary>
        public static async Task<string> PasteMessage(string message, string syntax)
        {
            WebRequest request = WebRequest.Create(PASTE_EE_URL);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add($"X-Auth-Token: { PASTE_EE_API_KEY }");

            PasteRequest bodyData = new PasteRequest()
            {
                Encrypted = false,
                Description = "Created by BrackeysBot, due to massive codeblock on discord.gg/brackeys.",
                Sections = new PasteRequest.Section[]
                {
                    new PasteRequest.Section()
                    {
                        Name = "Massive Codeblock",
                        Syntax = string.IsNullOrEmpty(syntax) ? "autodetect" : syntax,
                        Contents = message
                    }
                }
            };
            string bodyString = JsonConvert.SerializeObject(bodyData, Formatting.None);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(bodyString);

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bodyBytes, 0, bodyBytes.Length);
            dataStream.Close();

            WebResponse response = await request.GetResponseAsync();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseString = await reader.ReadToEndAsync();

            reader.Close();
            dataStream.Close();
            response.Close();

            PasteResponse responseData = JsonConvert.DeserializeObject<PasteResponse>(responseString);
            return responseData.Link;
        }

        /// <summary>
        /// Hastes the specified message, given that it meets the massive codeblock requirements.
        /// </summary>
        public static async Task PasteIfMassiveCodeblock(IMessage message)
        {
            string content = message.Content;
            if (HasCodeblockFormat(content) && content.Length >= MASSIVE_THRESHOLD)
            {
                RemoveCodeblockFormat(ref content, out string syntax);
                string url = await PasteMessage(content, syntax);

                await message.Channel.SendMessageAsync($"Paste created in place of massive codeblock by { ((IGuildUser)message.Author).GetDisplayName() }: <{ url }>");
                await message.DeleteAsync();
            }
        }

        /// <summary>
        /// Checks if the specified message is in a codeblock format.
        /// </summary>
        public static bool HasCodeblockFormat(string message)
        {
            return _codeblockRegex.IsMatch(message);
        }

        /// <summary>
        /// Removes the codeblock format (```) from a string, and potentially retrieves the formatting of the code.
        /// </summary>
        public static void RemoveCodeblockFormat (ref string message, out string syntax)
        {
            var matches = _codeblockRegex.Matches(message);
            syntax = "";
            while (matches.Count > 0)
            {
                var groups = matches[0].Groups;
                if (groups.Count == 2)
                {
                    // There is a codeblock format, but no syntax passed
                    message = message.Remove(matches[0].Index, matches[0].Length).Insert(matches[0].Index, groups[1].Value + "\n");
                }
                else if (groups.Count == 3)
                {
                    // There is a codeblock format, and a syntax has been passed
                    syntax = groups[1].Value;
                    message = message.Remove(matches[0].Index, matches[0].Length).Insert(matches[0].Index, groups[2].Value + "\n");
                }
                matches = _codeblockRegex.Matches(message);
            }
            message = message.Replace(CODEBLOCK_IDENTIFIER, string.Empty);
        }
    }
}
