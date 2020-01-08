using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Services;

using Humanizer;

namespace BrackeysBot.Commands
{
    public sealed partial class ModerationModule : BrackeysBotModule
    {
        private const string _defaultMuteReason = "Unspecified.";

        [Command("mute")]
        [Summary("Mutes a member, with an optional reason and duration.")]
        [Remarks("mute <user> [duration] [reason]")]
        [Priority(1)]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(
            [Summary("The user to mute.")] SocketGuildUser user,
            [Summary("The duration for the mute."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration,
            [Summary("The reason why to mute the user."), Remainder] string reason = _defaultMuteReason)
            => await TempmuteAsync(user, duration, reason);

        [Command("mute")]
        [Summary("Mutes a member, with an optional reason.")]
        [Remarks("mute <user> [reason]")]
        [HideFromHelp]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(
            [Summary("The user to mute.")] SocketGuildUser user,
            [Summary("The reason why to mute the user."), Remainder] string reason = _defaultMuteReason)
        {
            await user.MuteAsync(Context);
            await ReplyAsync($"I muted {user.Mention} because of **{reason}**.");

            Moderation.AddInfraction(user, Infraction.Create(Moderation.RequestInfractionID())
                .WithType(InfractionType.Mute)
                .WithModerator(Context.User)
                .WithDescription(reason));
            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Mute)
                .WithTarget(user)
                .WithReason(reason));
        }

        [Command("tempmute")]
        [Summary("Temporarily mutes a member for the specified duration, for the optional reason.")]
        [Remarks("tempmute <user> <duration> [reason]")]
        [Priority(1)]
        [HideFromHelp]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task TempmuteAsync(
            [Summary("The user to mute.")] SocketGuildUser user,
            [Summary("The duration for the mute."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration,
            [Summary("The reason why to mute the user."), Remainder] string reason = _defaultMuteReason)
        {
            await user.MuteAsync(Context);
            await ReplyAsync($"I muted {user.Mention} for {duration.Humanize(7)} because of **{reason}**.");

            Moderation.AddTemporaryInfraction(TemporaryInfractionType.TempMute, user, Context.User, duration, reason);
            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.TempMute)
                .WithTarget(user)
                .WithDuration(duration)
                .WithReason(reason));
        }

        [Command("unmute")]
        [Summary("Unmutes a user.")]
        [Remarks("unmute <user>")]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync(
            [Summary("The user to unmute.")] SocketGuildUser user)
        {
            await user.UnmuteAsync(Context);
            await ReplyAsync($"I unmuted {user.Mention}!");

            Moderation.ClearTemporaryInfraction(TemporaryInfractionType.TempMute, user);
            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Unmute)
                .WithTarget(user));
        }
    }
}
