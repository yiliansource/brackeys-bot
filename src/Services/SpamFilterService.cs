using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace BrackeysBot.Services
{
    public class SpamFilterService : BrackeysBotService, IInitializeableService
    {
        private const int maxConsecutiveSpamWords = 3;
        private const int maxSumSpamWords = 6;

        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;
        private readonly ModerationService _moderationService;
        private readonly ModerationLogService _loggingService;

        public SpamFilterService(
            DiscordSocketClient discord,
            DataService dataService,
            ModerationService moderationService,
            ModerationLogService loggingService)
        {
            _discord = discord;
            _dataService = dataService;
            _moderationService = moderationService;
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
            const int muteDurationSeconds = 30;

            if (TriggersSpamWordThreshold(content))
            {
                await MuteUserTemporary(s as SocketUserMessage, muteDurationSeconds);
                NotifyUser(s);
            }   
        }

        public async Task CheckEditedMessageAsync(Cacheable<IMessage, ulong> cacheable, SocketMessage s, ISocketMessageChannel channel)
        {
            if (!s.EditedTimestamp.HasValue)
                return;

            await CheckMessageAsync(s);
        }

        private bool TriggersSpamWordThreshold(string msg)
        {
            string[] spamWords = _dataService.Configuration.SpamWords;

            if (spamWords == null)
                return false;

            return spamWords.Any(str => ContainsMultipleInARow(msg, str, maxConsecutiveSpamWords) || ContainsMultiple(msg, str, maxSumSpamWords));
        }

        private bool ContainsMultipleInARow(string msg, string searchTxt, int minAmount)
        {
            string regexSearchTxt = $".*({Regex.Escape(searchTxt)}){{{minAmount}}}.*";
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

        private async Task MuteUserTemporary(SocketMessage s, int durationSeconds)
        {
            SocketGuildUser target = s.Author as SocketGuildUser;
            // If target is already muted due to other services, skip this mute
            if (!target.IsMuted)
            {
                await target.ModifyAsync(x => x.Mute = true);
                TimedUnmute(target, durationSeconds * 1000);
            }
        }

        private async void TimedUnmute(SocketGuildUser user, int milliseconds)
        {
            await Task.Delay(milliseconds);
            await user.ModifyAsync(x => x.Mute = false);
        }

        private async void NotifyUser(SocketMessage s)
        {
            IMessage msg = (await s.Channel.SendMessageAsync($"Hey {s.Author.Id.Mention()}! Your message contains too many of the same words flagged as spam, if you believe this is an error contact a Staff member!")) as IMessage;
            msg.TimedDeletion(5000);
        }
    }
}