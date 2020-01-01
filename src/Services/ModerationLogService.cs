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
        private readonly LoggingService _log;

        public ModerationLogService(DataService data, LoggingService log)
        {
            _data = data;
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

            ITextChannel channel = await logEntry.Context.Guild.GetTextChannelAsync(moderationLogChannelID);
            await embed.SendToChannel(channel);
        }

        private Embed CreateEmbedFromLogEntry(ModerationLogEntry logEntry)
            => new EmbedBuilder()
                .WithAuthor(logEntry.ActionType.ToString().Envelop("[", "]"), logEntry.Target?.GetAvatarUrl())
                .AddField("Moderator", logEntry.Moderator.Mention, true)
                .AddFieldConditional(logEntry.Target != null, "User", logEntry.Target.Mention, true)
                .AddFieldConditional(!string.IsNullOrEmpty(logEntry.Reason), "Reason", logEntry.Reason, true)
                .AddFieldConditional(logEntry.Channel != null, "Channel", logEntry.Channel.Mention, true)
                .AddFieldConditional(logEntry.Duration != null, "Duration", (logEntry.Duration ?? TimeSpan.Zero).Humanize(7), true)
                .WithFooter($"{logEntry.Time.ToTimeString()} | {logEntry.Time.ToDateString()}")
                .Build();
    }
}
