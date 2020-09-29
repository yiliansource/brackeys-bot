using System;
using System.Threading.Tasks;
using BrackeysBot.Core.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public sealed partial class ModerationModule : BrackeysBotModule
    {
        [Command("mute")]
        [Summary("Mutes a member, with an optional reason and duration.")]
        [Remarks("mute <user> [duration] [reason]")]
        [Priority(1)]
        [RequireHelper]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(
            [Summary("The user to mute.")] GuildUserProxy user,
            [Summary("The duration for the mute."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration,
            [Summary("The reason why to mute the user."), Remainder] string reason = DefaultReason)
            => await TempmuteAsync(user, duration, reason);

        [Command("mute")]
        [Summary("Mutes a member, with an optional reason.")]
        [Remarks("mute <user> [reason]")]
        [HideFromHelp]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(
            [Summary("The user to mute.")] GuildUserProxy user,
            [Summary("The reason why to mute the user."), Remainder] string reason = DefaultReason)
        {
            if (!user.HasValue)
                throw new ArgumentException($"User with ID {user.ID} is not in the server!");

            await user.GuildUser.MuteAsync(Context);

            SetUserMuted(user.ID, true);

            Infraction infraction = Infraction.Create(Moderation.RequestInfractionID())
                .WithType(InfractionType.Mute)
                .WithModerator(Context.User)
                .WithDescription(reason);

            Moderation.AddInfraction(user.GuildUser, infraction);
            
            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithInfractionId(infraction.ID)
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Mute)
                .WithTarget(user)
                .WithReason(reason), Context.Channel);
        }

        [Command("tempmute")]
        [Summary("Temporarily mutes a member for the specified duration, for the optional reason.")]
        [Remarks("tempmute <user> <duration> [reason]")]
        [Priority(1)]
        [HideFromHelp]
        [RequireHelper]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task TempmuteAsync(
            [Summary("The user to mute.")] GuildUserProxy user,
            [Summary("The duration for the mute."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration,
            [Summary("The reason why to mute the user."), Remainder] string reason = DefaultReason)
        {
            if (!user.HasValue)
                throw new ArgumentException($"User with ID {user.ID} is not in the server!");

            bool unlimitedTime = (Context.User as IGuildUser).GetPermissionLevel(Data.Configuration) >= PermissionLevel.Moderator;
            double givenDuration = duration.TotalMilliseconds;
            int maxHelperMuteDuration = Data.Configuration.HelperMuteMaxDuration;

            if (!unlimitedTime && givenDuration > maxHelperMuteDuration) {
                duration = TimeSpan.FromMilliseconds(maxHelperMuteDuration);
            }

            await user.GuildUser.MuteAsync(Context);

            SetUserMuted(user.ID, true);

            Infraction infraction = Moderation.AddTemporaryInfraction(TemporaryInfractionType.TempMute, user.GuildUser, Context.User, duration, reason);
            
            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithInfractionId(infraction.ID)
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.TempMute)
                .WithTarget(user)
                .WithDuration(duration)
                .WithReason(reason), Context.Channel);
        }

        [Command("unmute")]
        [Summary("Unmutes a user.")]
        [Remarks("unmute <user>")]
        [RequireHelper]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync(
            [Summary("The user to unmute.")] GuildUserProxy user)
        {
            if (!user.HasValue)
                throw new ArgumentException($"User with ID {user.ID} is not in the server!");

            await user.GuildUser.UnmuteAsync(Context);

            SetUserMuted(user.ID, false);

            Moderation.ClearTemporaryInfraction(TemporaryInfractionType.TempMute, user.GuildUser);
            
            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Unmute)
                .WithTarget(user), Context.Channel);
        }

        private void SetUserMuted(ulong id, bool muted) {
            Data.UserData.GetOrCreate(id).Muted = muted;
            Data.SaveUserData();
        }
    }
}
