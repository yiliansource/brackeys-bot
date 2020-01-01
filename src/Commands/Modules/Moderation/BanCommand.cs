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
        public ModerationService Moderation { get; set; }

        private const string _defaultReason = "Unspecified.";
        private const int _pruneDays = 7;

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
        [Summary("Bans a member from the server, with an optional reason.")]
        [Remarks("ban <user> [reason]")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(
            [Summary("The user to ban.")] SocketGuildUser user,
            [Summary("The reason why to ban the user."), Remainder] string reason = _defaultReason)
        {
            await user.TrySendMessageAsync($"You were banned from **{Context.Guild.Name}** because of {reason}.");

            await user.BanAsync(_pruneDays, reason);
            await ReplyAsync($"I banned {user.Mention} because of **{reason}**.");

            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Ban)
                .WithTarget(user)
                .WithReason(reason));
        }

        [Command("ban")]
        [Summary("Bans a member from the server by his ID, with an optional reason.")]
        [Remarks("ban <userId> [reason]")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(
            [Summary("The user ID to ban.")] ulong userId,
            [Summary("The reason why to ban the user."), Remainder] string reason = _defaultReason)
        {
            await Context.Guild.AddBanAsync(userId, _pruneDays, reason);
            await ReplyAsync($"I banned <@{userId}> because of **{reason}**.");

            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Ban)
                .WithTarget(userId)
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
            await user.TrySendMessageAsync($"You were banned from **{Context.Guild.Name}** for {duration.Humanize(7)} because of **{reason}**.");

            await user.BanAsync(_pruneDays, reason);
            await ReplyAsync($"I banned {user.Mention} for {duration.Humanize(7)} because of **{reason}**.");

            Moderation.RegisterTemporaryBan(user, Context.User, DateTime.Now.Add(duration), reason);
            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.TempBan)
                .WithTarget(user)
                .WithDuration(duration)
                .WithReason(reason));
        }
    }
}
