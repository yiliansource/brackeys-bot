using System;
using System.Collections.Generic;
using System.Text;
using BrackeysBot.Core.Models;
using Discord;
using Discord.Commands;

namespace BrackeysBot
{
    public struct ModerationLogEntry
    {
        public int InfractionId { get; set; }
        public ICommandContext Context { get; set; }
        public IUser Moderator { get; set; }
        public IUser Target { get; set; }
        public ulong TargetID { get; set; }
        public ITextChannel Channel { get; set; }
        public ModerationActionType ActionType { get; set; }
        public string Reason { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTimeOffset Time { get; set; }
        public string AdditionalInfo { get; set; }
        public static ModerationLogEntry New
            => new ModerationLogEntry() { InfractionId = -1 };

        public bool HasTarget => Target != null || TargetID != 0;
        public string TargetMention => Target != null
            ? Target.Mention
            : $"<@{TargetID}>";

        public ModerationLogEntry WithInfractionId(int infrId) 
        {
            InfractionId = infrId;
            return this;
        }

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

        public ModerationLogEntry WithTarget(GuildUserProxy target) 
            => target.HasValue ? WithTarget(target.GuildUser) : WithTarget(target.ID);

        public ModerationLogEntry WithTarget(IUser target)
        {
            Target = target;
            TargetID = target.Id;
            return this;
        }
        public ModerationLogEntry WithTarget(ulong id)
        {
            TargetID = id;
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

        public ModerationLogEntry WithAdditionalInfo(string info) 
        {
            AdditionalInfo = info;
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
