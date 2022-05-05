using System;
using System.Web;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Text;
using PasteMystNet;

namespace BrackeysBot.Services
{
    public class CodeblockService : BrackeysBotService, IInitializeableService
    {
        private readonly DiscordSocketClient _client;
        private readonly DataService _data;
        private readonly LoggingService _logger;

        private static readonly Regex _codeblockRegex = new Regex(@"^(?:\`){1,3}(\w+?)?\n([^\`]*)\n?(?:\`){1,3}$", RegexOptions.Singleline);

        public CodeblockService(DiscordSocketClient client, DataService data, LoggingService logger)
        {
            _client = client;
            _data = data;
            _logger = logger;
        }

        public void Initialize()
        {
            _client.MessageReceived += CheckMessage;
        }

        private async Task CheckMessage(SocketMessage sm)
        {
            if (!(sm is SocketUserMessage msg) || msg.Author.IsBot)
                return;

            ulong[] allowedChannels = _data.Configuration.AllowedCodeblockChannelIDs;
            if (allowedChannels != null && allowedChannels.Contains(msg.Channel.Id))
                return;

            int msgWrappedLineCount = msg.Content
                .Split(new[] {'\n', '\r'})
                .Select(l => (int)Math.Ceiling(l.Length / 47f)) // This could be turned into parameter, currently it's amount of characters that fit on mobile with 100% scale
                .Sum();

            if (msgWrappedLineCount > _data.Configuration.CodeblockThreshold && HasCodeblockFormat(msg.Content))
            {
                string url = await PasteMessage(msg);

                if (url is null)
                {
                    await new EmbedBuilder()
                        .WithDescription($"The message couldn't be auto pasted. If this happens again please notify Staff. [check the logs].")
                        .WithColor(Color.Red)
                        .Build()
                        .SendToChannel(msg.Channel);

                    return;
                }

                await new EmbedBuilder()
                    .WithAuthor("Pasted!", msg.Author.EnsureAvatarUrl(), url)
                    .WithDescription($"Massive codeblock by {msg.Author.Mention} was pasted!\n[Click here to view it!]({url})")
                    .WithColor(Color.Green)
                    .Build()
                    .SendToChannel(msg.Channel);

                await msg.DeleteAsync();
            }
        }

        public async Task<string> PasteMessage(IMessage msg)
        {
            string code = msg.Content;
            string lang = "Autodetect";
            if (HasCodeblockFormat(code))
            {
                code = ExtractCodeblockContent(code, out lang);
            }

            var langRes = await PasteMystLanguage.GetLanguageByNameAsync(lang);

            if (langRes == null)
            {
                lang = "Autodetect";
            }

            var paste = new PasteMystPasteForm
            {
                Title = $"paste by {msg.Author.Username}#{msg.Author.Discriminator} [BrackeysBot]",
                // ExpireDuration = "never", // default is "never"
                Pasties = new[]
                {
                    new PasteMystPastyForm
                    {
                        Title = "(untitled)",
                        // remove trailing newline
                        Code = code.TrimEnd( Environment.NewLine.ToCharArray()),
                        Language = lang,
                    },
                },
            };

            try
            {
                var res = await paste.PostPasteAsync();

                return $"https://paste.myst.rs/{res.Id}";
            }
            catch (Exception e)
            {
                await _logger.LogMessageAsync(new LogMessage(LogSeverity.Warning, "Paste", $"failed to paste: {e}"));


                return null;
            }
        }

        private static bool HasCodeblockFormat(string content)
            => _codeblockRegex.IsMatch(content);
        private static string ExtractCodeblockContent(string content)
            => ExtractCodeblockContent(content, out string _);
        private static string ExtractCodeblockContent(string content, out string lang)
        {
            Match m = _codeblockRegex.Match(content);
            if (m.Success)
            {
                // If 2 capture is present, the message only contains content, no lang
                if (m.Groups.Count == 2)
                {
                    lang = "Autodetect";
                    return m.Groups[1].Value;
                }
                // If 3 captures are present, the message contains content and a lang
                if (m.Groups.Count == 3)
                {
                    lang = m.Groups[1].Value;
                    return m.Groups[2].Value;
                }
            }

            lang = null;
            return null;
        }
    }
}
