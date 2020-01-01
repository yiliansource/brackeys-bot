using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Humanizer;

namespace BrackeysBot.Services
{
    public class ModerationLogService : BrackeysBotService
    {
        private readonly DataService _data;
        private readonly DiscordSocketClient _client;
        private readonly LoggingService _log;

        public ModerationLogService(DataService data, DiscordSocketClient client, LoggingService log)
        {
            _data = data;
            _client = client;
            _log = log;
        }

        public void CreateEntry(ModerationLogEntry logEntry)
        {
            _ = _log.LogMessageAsync(new LogMessage(LogSeverity.Info, "ModLog", $"{logEntry.Moderator} performed {logEntry.ActionType}."));
            _ = PostToLogChannelAsync(logEntry);
        }

        private async Task PostToLogChannelAsync(ModerationLogEntry logEntry)
        {
            ulong moderationLogChannelID = _data.Configuration.ModerationLogChannel;
            if (moderationLogChannelID == 0)
                throw new Exception("Invalid moderation log channel ID.");

            Embed embed = CreateEmbedFromLogEntry(logEntry);

            IGuild guild = _client.GetGuild(_data.Configuration.Guild);
            ITextChannel channel = await guild.GetTextChannelAsync(moderationLogChannelID);
            await embed.SendToChannel(channel);
        }

        private Embed CreateEmbedFromLogEntry(ModerationLogEntry logEntry)
            => new EmbedBuilder()
                .WithAuthor(logEntry.ActionType.Humanize(), logEntry.Target?.GetAvatarUrl())
                .WithColor(GetColorForAction(logEntry.ActionType))
                .AddField("Moderator", logEntry.Moderator.Mention, true)
                .AddFieldConditional(logEntry.HasTarget, "User", logEntry.TargetMention, true)
                .AddFieldConditional(!string.IsNullOrEmpty(logEntry.Reason), "Reason", logEntry.Reason, true)
                .AddFieldConditional(logEntry.Channel != null, "Channel", logEntry.Channel?.Mention, true)
                .AddFieldConditional(logEntry.Duration != null, "Duration", (logEntry.Duration ?? TimeSpan.Zero).Humanize(7), true)
                .WithFooter($"{logEntry.Time.ToTimeString()} | {logEntry.Time.ToDateString()}")
                .Build();

        private Color GetColorForAction(ModerationActionType actionType)
        {
            switch (actionType)
            {
                case ModerationActionType.Ban:
                case ModerationActionType.Mute:
                case ModerationActionType.TempBan:
                case ModerationActionType.TempMute:
                case ModerationActionType.Kick:
                    return Color.Red;

                case ModerationActionType.Unban:
                case ModerationActionType.Unmute:
                    return Color.Green;

                case ModerationActionType.ClearMessages:
                    return new Color(1, 1, 1);

                default:
                    return new Color(0, 0, 0);
            }
        }
    }
}
