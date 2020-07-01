using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.WebSocket;
using BrackeysBot.Commands;

namespace BrackeysBot.Services
{
    public class SpamFilterService : BrackeysBotService, IInitializeableService
    {
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

            string content = msg.Content;
            SpamFilterConfiguration spamConfiguration = _dataService.Configuration.SpamSettings ?? new SpamFilterConfiguration();

            if (TriggersSpamWordThreshold(content, spamConfiguration))
            {
                await TempMuteUser(s as SocketUserMessage, content, spamConfiguration.MuteDuration);
            }   
        }

        public async Task CheckEditedMessageAsync(Cacheable<IMessage, ulong> cacheable, SocketMessage s, ISocketMessageChannel channel)
        {
            if (!s.EditedTimestamp.HasValue)
                return;

            await CheckMessageAsync(s);
        }

        private bool TriggersSpamWordThreshold(string msg, SpamFilterConfiguration spamConfiguration)
        {
            string[] spamWords = _dataService.Configuration.SpamWords;

            if (spamWords == null)
                return false;

            return spamWords.Any(str => ContainsMultipleInARow(msg, str, spamConfiguration.ConsecutiveWordThreshold) || ContainsMultiple(msg, str, spamConfiguration.FullMessageWordThreshold));
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
            string reason = "User message triggered the spam filter";
            _moderationService.AddTemporaryInfraction(TemporaryInfractionType.TempMute, user, _discord.CurrentUser, muteDuration, reason);

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