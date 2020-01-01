using System;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;

namespace BrackeysBot
{
    public struct ModerationLogEntry
    {
        public ICommandContext Context { get; set; }
        public IUser Moderator { get; set; }
        public IUser Target { get; set; }
        public ITextChannel Channel { get; set; }
        public ModerationActionType ActionType { get; set; }
        public string Reason { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTimeOffset Time { get; set; }

        public static ModerationLogEntry New
            => new ModerationLogEntry();

        public ModerationLogEntry WithContext(ICommandContext context)
        {
            Context = context;
            return this;
        }
        public ModerationLogEntry WithModerator(IUser moderator)
        {
            Moderator = moderator;
            return this;
        }
        public ModerationLogEntry WithChannel(ITextChannel channel)
        {
            Channel = channel;
            return this;
        }
        public ModerationLogEntry WithActionType(ModerationActionType actionType)
        {
            ActionType = actionType;
            return this;
        }
        public ModerationLogEntry WithTarget(IUser target)
        {
            Target = target;
            return this;
        }
        public ModerationLogEntry WithReason(string reason)
        {
            Reason = reason;
            return this;
        }
        public ModerationLogEntry WithDuration(TimeSpan duration)
        {
            Duration = duration;
            return this;
        }
        public ModerationLogEntry WithTime(DateTimeOffset time)
        {
            Time = time;
            return this;
        }
        
        public ModerationLogEntry WithDefaultsFromContext(ICommandContext context)
        {
            return WithContext(context)
                .WithModerator(context.User)
                .WithTime(DateTimeOffset.Now);
        }
    }
}
