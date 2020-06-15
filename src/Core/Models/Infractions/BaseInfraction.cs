using System;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace BrackeysBot.Models.Database
{
    [NotMapped]
    public class BaseInfraction : Infraction
    {
        public BaseInfraction()
        {

        }

        public BaseInfraction(IUser target, IUser moderator, InfractionType type, string reason = "Undefined") 
        {
            base.TargetUserId = target.Id;
            base.ModeratorUserId = moderator.Id;
            base.ModerationTypeId = (int) type;
            base.Date = DateTime.UtcNow;
            base.Reason = reason;

            base.Target = target;
            base.Moderator = moderator;
        }

        public BaseInfraction(ulong targetId, IUser moderator, InfractionType type, string reason = "Undefined") 
        {
            base.TargetUserId = targetId;
            base.ModeratorUserId = moderator.Id;
            base.ModerationTypeId = (int) type;
            base.Date = DateTime.UtcNow;
            base.Reason = reason;

            base.Moderator = moderator;
        }
    }

    public static class InfractionsCreator
    {
        public static Infraction Ban(ulong target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Ban, reason);

        public static Infraction Ban(IUser target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Ban, reason);

        public static Infraction TempBan(ulong target, IUser moderator, TimeSpan duration, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.TemporaryBan, reason) { Duration = duration };

        public static Infraction TempBan(IUser target, IUser moderator, TimeSpan duration, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.TemporaryBan, reason) { Duration = duration };

        public static Infraction Mute(ulong target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Mute, reason);

        public static Infraction Mute(IUser target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Mute, reason);

        public static Infraction Kick(ulong target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Kick, reason);

        public static Infraction Kick(IUser target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Kick, reason);

        public static Infraction Warn(ulong target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Warning, reason);

        public static Infraction Warn(IUser target, IUser moderator, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.Warning, reason);

        public static Infraction TempMute(ulong target, IUser moderator, TimeSpan duration, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.TemporaryMute, reason) { Duration = duration };

        public static Infraction TempMute(IUser target, IUser moderator, TimeSpan duration, string reason = "Undefined")
            => new BaseInfraction(target, moderator, InfractionType.TemporaryMute, reason) { Duration = duration };
    }
}