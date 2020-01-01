using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public sealed partial class ModerationModule : BrackeysBotModule
    {
        private const string _defaultReason = "Unspecified.";

        [Command("ban")]
        [Summary("Bans a member from the server, with an optional reason and duration.")]
        [Remarks("ban <user> [duration] [reason]")]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(
            [Summary("The user to ban.")] SocketGuildUser user,
            [Summary("The duration for the ban."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration,
            [Summary("The reason why to ban the user."), Remainder] string reason = _defaultReason)
            => await TempbanAsync(user, duration, reason);

        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(
            SocketGuildUser user,
            [Remainder] string reason = _defaultReason)
        {
            await ReplyAsync($"Banned {user} for the reason '{reason}'.");

            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Ban)
                .WithTarget(user)
                .WithReason(reason));
        }

        [Command("tempban")]
        [Summary("Temporarily bans a member from the server, with an optional reason.")]
        [Remarks("tempban <user> <duration> [reason]")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task TempbanAsync(
            [Summary("The user to temporarily ban.")] SocketGuildUser user, 
            [Summary("The duration for the ban."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration, 
            [Summary("The reason why to ban the user."), Remainder] string reason = _defaultReason)
        {
            await ReplyAsync($"Banned {user} for {duration} for the reason '{reason}'.");

            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.TempBan)
                .WithTarget(user)
                .WithDuration(duration)
                .WithReason(reason));
        }
    }
}
