using System;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.Core.Models;
using BrackeysBot.Models.Database;
using BrackeysBot.Services;
using Discord;
using Discord.WebSocket;
using Humanizer;

namespace BrackeysBot.Managers
{
    public class InfractionManager : BrackeysBotService
    {
        private const string INFRACTION_NOT_FOUND_MESSAGE = "Infraction with id {0} does not exist!";
        private const string INFRACTION_DELETED_MESSAGE = "Infraction with id {0} is deleted from <@{1}>!";

        private readonly DiscordSocketClient _client;
        private readonly BotConfiguration _config;
        private readonly DatabaseService _db;
        private readonly LoggingService _logging;
        private readonly ModerationLogService _moderationLog;

        public InfractionManager(DiscordSocketClient client, DataService data, DatabaseService db, LoggingService logging, ModerationLogService moderationLog) 
        {
            _client = client;
            _config = data.Configuration;
            _db = db;
            _logging = logging;
            _moderationLog = moderationLog;
        }

        public Infraction CreateInfraction(ulong targetId, IUser moderator, InfractionType infractionType, TimeSpan? duration, string reason = "Undefined")
            => new BaseInfraction(targetId, moderator, infractionType, reason) {
                Duration = duration
            };

        public Infraction CreateInfraction(IUser target, IUser moderator, InfractionType infractionType, TimeSpan? duration, string reason = "Undefined")
            => new BaseInfraction(target, moderator, infractionType, reason) {
                Duration = duration
            };

        public void SaveMessage(IUserMessage msg, MessageActionType msgActionType, Infraction infr = null)
        {
            LoggedMessage logged = new LoggedMessage {
                UserId = msg.Author.Id,
                DiscordChannelId = msg.Channel.Id,
                DiscordMessageId = msg.Id,
                MsgContent1 = msg.Content,
                MsgAction = (int) msgActionType
            };

            _db.StoreMessage(logged, infr);
        }

        public async Task AddInfraction(IMessageChannel channel, Infraction infraction, bool messageUser = false)
        {
            Infraction[] previousInfractions = _db.GetAllInfractionsOfUser(infraction.TargetUserId);

            foreach (Infraction infr in previousInfractions)
            {
                TemporaryInfraction tempInfr = _db.GetActiveTemporaryInfractionForInfraction(infr.Id);
                if (tempInfr != null) 
                    infr.Duration = tempInfr.EndDate - infr.Date;
            }

            _db.AddInfraction(infraction as Infraction);

            ModerationLogEntry logEntry = ModerationLogEntry.New
                .WithId(infraction.Id)
                .WithModerator(infraction.Moderator)
                .WithActionType(InfractionTypeToModerationActionType((InfractionType) infraction.ModerationTypeId))
                .WithReason(infraction.Reason)
                .WithTime(DateTimeOffset.UtcNow);

            if (infraction.Target != null)
                logEntry.WithTarget(infraction.Target);
            else 
                logEntry.WithTarget(_client.TryGetUser(_config.GuildID, infraction.TargetUserId)); 
            
            if (previousInfractions.Length > 0) 
            {
                String additional = string.Join('\n', previousInfractions.OrderByDescending(i => i.Date).Select(i => i.ToString()));
                logEntry.WithAdditionalInfo(additional);
            }

            if (infraction.Duration.HasValue) 
            {
                _db.AddTempInfraction(infraction, infraction.Duration.Value);
                logEntry.WithDuration(infraction.Duration.Value);
            }

            if (messageUser)
                SendInfractionMessageToUser(infraction, previousInfractions.Length + 1);

            await _moderationLog.CreateEntry(logEntry, channel);

            await _logging.LogMessageAsync(new LogMessage(LogSeverity.Debug, "Infraction", InfractionToString(infraction)));
        }

        public async Task RemoveInfractionIfPresent(IMessageChannel channel, int infractionId)
        {
            Infraction infraction = _db.GetInfraction(infractionId);

            if (infraction == null)
            {
                await new EmbedBuilder().WithDescription(String.Format(INFRACTION_NOT_FOUND_MESSAGE, infractionId)).Build().SendToChannel(channel);
                return;
            }

            _db.RemoveInfraction(infraction);

            ModerationLogEntry logEntry = ModerationLogEntry.New
                .WithId(infraction.Id)
                .WithModerator(infraction.Moderator)
                .WithActionType(ModerationActionType.DeletedInfraction)
                .WithTime(DateTimeOffset.UtcNow);

            if (infraction.Target != null)
                logEntry.WithTarget(infraction.Target);
            else 
                logEntry.WithTarget(_client.TryGetUser(_config.GuildID, infraction.TargetUserId));

            await _moderationLog.CreateEntry(logEntry);

            await _logging.LogMessageAsync(new LogMessage(LogSeverity.Debug, "Infraction", InfractionToString(infraction)));

            await new EmbedBuilder().WithDescription(String.Format(INFRACTION_DELETED_MESSAGE, infractionId, infraction.Id)).Build().SendToChannel(channel);
        }

        public async Task<bool> RemoveActiveTemporaryInfractionIfPresent(IMessageChannel channel, ulong targetUserId, InfractionType type, IUser moderator) 
        {
            TemporaryInfraction[] infrs = _db.GetActiveTemporaryInfractionsOfUser(targetUserId, type);

            GuildUserProxy proxy = _client.TryGetUser(_config.GuildID, targetUserId);

            if (infrs.Length == 0) 
                return false;

            foreach (TemporaryInfraction infr in infrs)
            {
                _db.RemoveTemporaryInfraction(infr);

                await _logging.LogMessageAsync(new LogMessage(LogSeverity.Debug, "Removed Infraction", $"Source: {infr.InfractionId}, End: {infr.EndDate}"));
            }

            ModerationLogEntry logEntry = ModerationLogEntry.New
                .WithTarget(proxy)
                .WithModerator(moderator)
                .WithTime(DateTimeOffset.UtcNow);
            
            if (type == InfractionType.Ban || type == InfractionType.TemporaryBan)
                logEntry.WithActionType(ModerationActionType.Unban);
            else if (type == InfractionType.Mute || type == InfractionType.TemporaryMute)
                logEntry.WithActionType(ModerationActionType.Unmute);
            else 
                logEntry.WithActionType(ModerationActionType.Unknown);

            await _moderationLog.CreateEntry(logEntry);

            return true;
        }

        private static string InfractionToString(Infraction infraction) 
            => $"{infraction.ModeratorUserId} [{((InfractionType) infraction.ModerationTypeId).Humanize()}] -> {infraction.TargetUserId}: {infraction.Reason} ({(infraction.Duration.HasValue ? infraction.Duration.Value.Humanize() : "")})";

        private async void SendInfractionMessageToUser(Infraction infraction, int totalInfractionCount) 
        {
            string type = GetInfractionTypeString((InfractionType) infraction.ModerationTypeId);
            string duration = infraction.Duration.HasValue ? $" for **{infraction.Duration.Value.Humanize()}**" : "";
            string message = $"Hey there! You were **{type}**{duration} for **{infraction.Reason}**! You currently have **{totalInfractionCount}** infraction(s)!";

            await infraction.Target.TrySendMessageAsync(message);
        }

        private string GetInfractionTypeString(InfractionType type) 
        {
            switch (type) 
            {
                case InfractionType.Kick:
                    return "Kicked";
                case InfractionType.Mute:
                    return "Muted";
                case InfractionType.Warning:
                    return "Warned";
                case InfractionType.TemporaryMute:
                    return "Temporarily Muted";
                case InfractionType.TemporaryBan:
                    return "Temporarily Banned";
                case InfractionType.Ban:
                    return "Banned";
                default:
                    return "Given an Infraction";  
            }
        }

        private ModerationActionType InfractionTypeToModerationActionType(InfractionType type)
        {
            switch (type) 
            {
                case InfractionType.Kick:
                    return ModerationActionType.Kick;
                case InfractionType.Mute:
                    return ModerationActionType.Mute;
                case InfractionType.Warning:
                    return ModerationActionType.Warn;
                case InfractionType.TemporaryMute:
                    return ModerationActionType.TempMute;
                case InfractionType.TemporaryBan:
                    return ModerationActionType.TempBan;
                case InfractionType.Ban:
                    return ModerationActionType.Ban;
                default:
                    return ModerationActionType.Warn;
            }
        }  
    }
}