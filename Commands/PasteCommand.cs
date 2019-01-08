using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.Commands;
using System.IO;

using Newtonsoft.Json;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BrackeysBot.Commands
{
    [Serializable]
    public struct PasteMystCreateInfo
    {
        [JsonProperty ("code")]
        public string Code;
        [JsonProperty ("expiresIn")]
        public string ExpiresIn;
    }

    [Serializable]
    [JsonObject]
    public struct PasteMystResultInfo
    {
        [JsonProperty ("id")]
        public string Id;
        [JsonProperty ("createdAt")]
        public long CreatedAt;
        [JsonProperty ("expiresAt")]
        public string ExpiresAt;
        [JsonProperty ("code")]
        public string Code;
    }

    public class PasteCommand : ModuleBase
    {
        private const string API_BASEPOINT = "https://paste.myst.rs/api/";
        private const string PASTEMYST_BASE_URL = "https://paste.myst.rs/";
        private const int MASSIVE_THRESHOLD = 500;
        private const string CODEBLOCK_IDENTIFIER = "```";
        private static readonly Regex _codeblockRegex = new Regex($@"(?:{ CODEBLOCK_IDENTIFIER })(\w+)?\n([^{ CODEBLOCK_IDENTIFIER[0] }]*)", RegexOptions.Compiled);

        [Command("modpaste"), Alias("modhaste")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("modpaste <message_id>", "Paste a specific message to PasteMyst.")]
        public async Task ModPasteMessage(ulong messageId)
        {
            var message = await Context.Channel.GetMessageAsync(messageId);
            var messageContent = message.Content;
            RemoveCodeblockFormat(ref messageContent);
            string url = await CreatePaste(messageContent);

            await ReplyAsync($"Message by { message.Author.Mention } was pasted to <{ url }>.");
            await message.DeleteAsync();
        }

        [Command("paste"), Alias("haste")]
        [HelpData("paste <message>", "Pastes a message to PasteMyst.")]
        public async Task PasteMessage([Remainder] string messageContent)
        {
            string content = messageContent.Trim('\n', ' ');
            RemoveCodeblockFormat(ref content);
            string url = await CreatePaste(content);

            await ReplyAsync($"{ (Context.User as IGuildUser).GetDisplayName() }, I created a paste for you! <{ url }>");
            await Context.Message.DeleteAsync();
        }

        /// <summary>
        /// Hastes a string and returns the URL of the hastebin page.
        /// </summary>
        public static async Task<string> CreatePaste(string code)
        {
            PasteMystCreateInfo createInfo = new PasteMystCreateInfo
            {
                Code = HttpUtility.UrlPathEncode (code),
                ExpiresIn = "never"
            };

            HttpClient httpClient = new HttpClient ();
            httpClient.BaseAddress = new Uri (API_BASEPOINT);
            httpClient.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));
            httpClient.DefaultRequestHeaders.Connection.Add (HttpMethod.Post.ToString ().ToUpper ());

            StringContent content = new StringContent (JsonConvert.SerializeObject (createInfo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync ("paste", content);
            response.EnsureSuccessStatusCode ();
            string json = await response.Content.ReadAsStringAsync ();
            PasteMystResultInfo result = JsonConvert.DeserializeObject<PasteMystResultInfo> (json);
            return new Uri (new Uri (PASTEMYST_BASE_URL), result.Id).ToString ();
        }

        /// <summary>
        /// Hastes the specified message, given that it meets the massive codeblock requirements.
        /// </summary>
        public static async Task PasteIfMassiveCodeblock(IMessage message)
        {
            string content = message.Content;
            if (HasCodeblockFormat(content) && content.Length >= MASSIVE_THRESHOLD)
            {
                RemoveCodeblockFormat(ref content);
                string url = await CreatePaste(content);

                await message.Channel.SendMessageAsync($"Paste created in place of massive codeblock by { ((IGuildUser)message.Author).GetDisplayName() }: <{ url }>");
                await message.DeleteAsync();
            }
        }

        /// <summary>
        /// Checks if the specified message is in a codeblock format.
        /// </summary>
        public static bool HasCodeblockFormat(string message)
        {
            return _codeblockRegex.IsMatch(message)
                && message.StartsWith(CODEBLOCK_IDENTIFIER)
                && message.EndsWith(CODEBLOCK_IDENTIFIER);
        }

        /// <summary>
        /// Removes the codeblock format (```) from a string, and potentially retrieves the formatting of the code.
        /// </summary>
        public static void RemoveCodeblockFormat (ref string message)
        {
            var matches = _codeblockRegex.Matches(message);
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
                    message = message.Remove(matches[0].Index, matches[0].Length).Insert(matches[0].Index, groups[2].Value + "\n");
                }
                matches = _codeblockRegex.Matches(message);
            }
            message = message.Replace(CODEBLOCK_IDENTIFIER, string.Empty);
        }
    }
}
