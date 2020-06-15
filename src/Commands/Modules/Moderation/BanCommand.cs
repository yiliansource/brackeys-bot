using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Core.Models;

using Humanizer;
using BrackeysBot.Models.Database;

namespace BrackeysBot.Commands
{
    public sealed partial class ModerationModule : BrackeysBotModule
    {
        private const int _pruneDays = 7;

        [Command("ban")]
        [Summary("Bans a member from the server, with an optional reason and duration.")]
        [Remarks("ban <user> [duration] [reason]")]
        [Priority(1)]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(
            [Summary("The user to ban.")] SocketGuildUser user,
            [Summary("The duration for the ban."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration,
            [Summary("The reason why to ban the user."), Remainder] string reason = DefaultReason)
            => await TempbanAsync(user, duration, reason);

        [Command("ban")]
        [Summary("Bans a member from the server, with an optional reason.")]
        [Remarks("ban <user> [reason]")]
        [HideFromHelp]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(
            [Summary("The user to ban.")] GuildUserProxy user,
            [Summary("The reason why to ban the user."), Remainder] string reason = DefaultReason)
        {
            await Context.Guild.AddBanAsync(user.ID, 0, reason);

            Infraction infr;

            if (user.HasValue) 
                infr = InfractionsCreator.Ban(user.GuildUser, Context.User, reason);
            else 
                infr = InfractionsCreator.Ban(user.ID, Context.User, reason);

            await Infractions.AddInfraction(Context.Channel, infr, true);
        }

        [Command("tempban")]
        [Summary("Temporarily bans a member from the server, with an optional reason.")]
        [Remarks("tempban <user> <duration> [reason]")]
        [HideFromHelp]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task TempbanAsync(
            [Summary("The user to temporarily ban.")] SocketGuildUser user, 
            [Summary("The duration for the ban."), OverrideTypeReader(typeof(AbbreviatedTimeSpanTypeReader))] TimeSpan duration, 
            [Summary("The reason why to ban the user."), Remainder] string reason = DefaultReason)
        {
            await user.BanAsync(0, reason);

            await Infractions.AddInfraction(Context.Channel, InfractionsCreator.TempBan(user, Context.User, duration, reason), true);
        }

        [Command("unban")]
        [Summary("Removes the ban on a member, if possible.")]
        [Remarks("unban <userId>")]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task UnbanAsync(
            [Summary("The user ID to unban.")] GuildUserProxy user)
        {
            await Context.Guild.RemoveBanAsync(user.ID);

            EmbedBuilder builder = new EmbedBuilder();

            string username = user.HasValue ? user.GuildUser.Mention : $"<@{user.ID}>";

            if (await Infractions.RemoveActiveTemporaryInfractionIfPresent(Context.Channel, user.ID, InfractionType.TemporaryBan, Context.User))
                builder.WithDescription(String.Format("Unbanned {0}!", username));
            else 
                builder.WithDescription(String.Format("User {0} is not (temp)banned!", username));
            
            await builder.Build().SendToChannel(Context.Channel);
        }
    }
}
