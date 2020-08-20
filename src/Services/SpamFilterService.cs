using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.WebSocket;
using BrackeysBot.Commands;
using System.Collections.Generic;

namespace BrackeysBot.Services
{
    public class SpamFilterService : BrackeysBotService, IInitializeableService
    {
        private static readonly EmoteValueSet[] defaultEmoteMatches = DiscordDefaultEmoteData.EmoteMatchMap.Select(x => x.Value).ToArray();

        // Pattern of custom emotes is <:emote_name:emote_id> with the emote_id always having 18 digits (Twitter's snowflake for disord IDs)
        // Animated emotes start with an <a: instead
        // For emote names, discord seems to trim out all special characters for the real name, leaving just letters, numbers and underscore
        // default emotes from discord are unicode characters, so it only works on custom emotes for now
        private readonly Regex emoteRegex = new Regex(@"<a?:[A-Za-z0-9_]+:\d{18}>", RegexOptions.IgnoreCase);

        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;
        private readonly ModerationService _moderationService;
        private readonly IServiceProvider _provider;
        private readonly ModerationLogService _loggingService;

        public SpamFilterService(
            DiscordSocketClient discord,
            DataService dataService,
            ModerationService moderationService, 
            IServiceProvider provider, 
            ModerationLogService loggingService)
        {
            _discord = discord;
            _dataService = dataService;
            _moderationService = moderationService;
            _provider = provider;
            _loggingService = loggingService;
        }

        public void Initialize()
        {
            _discord.MessageReceived += CheckMessageAsync;
            _discord.MessageUpdated += CheckEditedMessageAsync;
        }
        
        public async Task CheckMessageAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || CanUseSpamWords(msg))
                return;

            SpamFilterConfiguration spamConfiguration = _dataService.Configuration.SpamSettings ?? new SpamFilterConfiguration();

            if (TriggersSpamThreshold(msg, spamConfiguration))
            {
                await TempMuteUser(s as SocketUserMessage, msg.Content, spamConfiguration.MuteDuration);
            }   
        }

        public async Task CheckEditedMessageAsync(Cacheable<IMessage, ulong> cacheable, SocketMessage s, ISocketMessageChannel channel)
        {
            if (!s.EditedTimestamp.HasValue)
                return;

            await CheckMessageAsync(s);
        }

        private bool TriggersSpamThreshold(SocketMessage msg, SpamFilterConfiguration spamConfiguration)
        {
            string[] spamWords = _dataService.Configuration.SpamWords;
            string msgContent = msg.Content;

            if (spamConfiguration.IncludeEmotes) {
                int emoteCount = emoteRegex.Matches(msgContent).Count;
                if (spamConfiguration.CheckForDefaultEmotes)
                    emoteCount += GetTotalDefaultEmoteCount(msgContent);
                if (emoteCount >= spamConfiguration.EmotesThreshold)
                    return true;
            }

            if (spamConfiguration.IncludeMentions)
            {
                int mentionsCount = msg.MentionedUsers.Count + msg.MentionedChannels.Count + msg.MentionedRoles.Count;
                if (mentionsCount >= spamConfiguration.MentionsThreshold)
                    return true;
            }
            if (spamWords != null && spamWords.Any(str => ContainsMultipleInARow(msgContent, str, spamConfiguration.ConsecutiveWordThreshold) || ContainsMultiple(msgContent, str, spamConfiguration.FullMessageWordThreshold)))
                return true;
            return false;
        }

        // A single common emote could previously result in multiple positives, assumingly because they might be a subset of other emotes
        // To fix this, the number of false matches that a single emote yields is taken into account and used it as divisor to get the "real" match count
        private int GetTotalDefaultEmoteCount(string msg)
        {
            float count = 0f;
            foreach (EmoteValueSet emoteInfo in defaultEmoteMatches)
            {
                // each match against emoteInfo.ValueRegexEscaped for a single emote only counts as one 
                // match, but we still need to divide it by the matchedSum against all emotes, so we have to use float to store the fraction
                count += (float)Regex.Matches(msg, emoteInfo.ValueRegexEscaped).Count / emoteInfo.RegexMatchCount;
            }
            return (int) (count + 0.01f);
        }

        private bool ContainsMultipleInARow(string msg, string searchTxt, int minAmount)
        {
            // Allow for whitespace characters after each occurence
            string regexSearchTxt = $".*({Regex.Escape(searchTxt)}\\s*){{{minAmount}}}.*";
            return Regex.IsMatch(msg, regexSearchTxt, RegexOptions.IgnoreCase);
        }

        private bool ContainsMultiple(string msg, string searchTxt, int minAmount)
        {
            string regexSearchTxt = Regex.Escape(searchTxt);
            return Regex.Matches(msg, regexSearchTxt, RegexOptions.IgnoreCase).Count >= minAmount;
        }

        private bool CanUseSpamWords(SocketUserMessage msg)
        {
            return msg.Author.IsBot || (msg.Author as IGuildUser).GetPermissionLevel(_dataService.Configuration) >= PermissionLevel.Guru;
        }

        private async Task TempMuteUser(SocketUserMessage s, string message, int durationSeconds)
        {
            SocketGuildUser user = s.Author as SocketGuildUser;
            BrackeysBotContext context = new BrackeysBotContext(s, _provider);

            await user.MuteAsync(context);
            SetUserMuted(user.Id, true);

            TimeSpan muteDuration = new TimeSpan(0, 0, durationSeconds);
            string reason = "triggering the spam filter with a recent message";
            _moderationService.AddTemporaryInfraction(TemporaryInfractionType.TempMute, user, _discord.CurrentUser, muteDuration, reason, skipPersistentInfractionRecord: true);

            string url = s.GetJumpUrl();

            await _loggingService.CreateEntry(ModerationLogEntry.New
                    .WithActionType(ModerationActionType.TempMute)
                    .WithTarget(user)
                    .WithReason($"Overused spam word: [Go to  message]({url})\n**{message}**")
                    .WithTime(DateTimeOffset.Now)
                    .WithModerator(_discord.CurrentUser));

            NotifyUser(s);
        }

        private void SetUserMuted(ulong id, bool muted)
        {
            _dataService.UserData.GetOrCreate(id).Muted = muted;
            _dataService.SaveUserData();
        }

        private async void NotifyUser(SocketMessage s)
        {
            IMessage msg = (await s.Channel.SendMessageAsync($"Hey {s.Author.Id.Mention()}! Your message contains too many occurences of a word flagged as spam, if you believe this is an error contact a Staff member!")) as IMessage;
            msg.TimedDeletion(5000);
        }
    }
}