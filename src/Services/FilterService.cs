using System.Threading.Tasks;
using System;
using System.Linq;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Services
{
    public class FilterService : BrackeysBotService, IInitializeableService
    {
        private readonly DiscordSocketClient _discord;
        private readonly DataService _dataService;
        private readonly ModerationService _moderationService;
        private readonly ModerationLogService _loggingService;

        public FilterService(
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
        }

        public async Task CheckMessageAsync(SocketMessage s) 
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot)
                return;

            string[] blockedWords = _dataService.Configuration.BlockedWords;

            string content = msg.Content;
            
            if (blockedWords.Any(str => content.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) >= 0)) {
                await DeleteMsgAndInfractUser(s, content);

            }
        }

        private async Task DeleteMsgAndInfractUser(SocketMessage s, string message)
        {
            SocketGuildUser target = s.Author as SocketGuildUser;

            await s.DeleteAsync();

            _moderationService.AddInfraction(target, 
                    Infraction.Create(_moderationService.RequestInfractionID())
                    .WithType(InfractionType.Warning)
                    .WithModerator(_discord.CurrentUser)
                    .WithAdditionalInfo($"**Message:** {message}")
                    .WithDescription("Used filtered word"));

            await _loggingService.CreateEntry(ModerationLogEntry.New
                    .WithActionType(ModerationActionType.Filtered)
                    .WithTarget(target)
                    .WithReason($"Deleted **{message}**")
                    .WithTime(DateTimeOffset.Now)
                    .WithModerator(_discord.CurrentUser));
        }
    }
}