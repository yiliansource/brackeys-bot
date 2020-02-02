using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Services;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public partial class SoftModerationModule : BrackeysBotModule
    {
        [Command("endorse"), Alias("star")]
        [Summary("Endorse a user and give them a star.")]
        [Remarks("endorse <id>")]
        [RequireGuru]
        [HideFromHelp]
        public async Task EndorseUserAsync(
            [Summary("The user ID to endorse.")] ulong id)
            => await EndorseUserAsync(await Context.Guild.GetUserAsync(id) as SocketGuildUser);

        [Command("endorse"), Alias("star")]
        [Summary("Endorse a user and give them a star.")]
        [Remarks("endorse <user>")]
        [RequireGuru]
        public async Task EndorseUserAsync(
            [Summary("The user to endorse.")] SocketGuildUser guildUser) 
        {
            UserData user = Data.UserData.GetOrCreate(guildUser.Id);
            user.Stars++;

            await new EmbedBuilder()
                .WithAuthor(guildUser)
                .WithColor(Color.Gold)
                .WithDescription($"Gave a :star: to {guildUser.Mention}! They now have {user.Stars} stars!")
                .Build()
                .SendToChannel(Context.Channel);
        }

        [Command("deleteendorse"), Alias("deletestar", "delstar", "delendorse")]
        [Summary("Remove a star from a user.")]
        [Remarks("deleteendorse <user>")]
        [RequireModerator]
        [HideFromHelp]
        public async Task DeleteEndorseUserAsync(
            [Summary("The user ID to remove an endorsement.")] ulong id)
            => await DeleteEndorseUserAsync(await Context.Guild.GetUserAsync(id) as SocketGuildUser);

        [Command("deleteendorse"), Alias("deletestar", "delstar", "delendorse")]
        [Summary("Remove a star from a user.")]
        [Remarks("deleteendorse <user>")]
        [RequireModerator]
        public async Task DeleteEndorseUserAsync(
            [Summary("The user to remove an endorsement.")] SocketGuildUser guildUser) 
        {
            UserData user = Data.UserData.GetOrCreate(guildUser.Id);

            EmbedBuilder builder = new EmbedBuilder();

            if (user.Stars == 0) {
                builder.WithColor(Color.Red).WithDescription("Can't remove a star, they have none!");
            } else {
                user.Stars--;
                builder.WithAuthor(guildUser).WithColor(Color.Green)
                    .WithDescription($"Took a :star: from {guildUser.Mention}! They now have {user.Stars} stars!");
            }

            await builder.Build()
                .SendToChannel(Context.Channel);
        }

        [Command("clearendorse"), Alias("clearstar", "clearstars")]
        [Summary("Remove all stars from a user.")]
        [Remarks("clearendorse <user>")]
        [RequireModerator]
        [HideFromHelp]
        public async Task WipeEndorseUserAsync(
            [Summary("The user ID to remove all endorsement.")] ulong id)
            => await WipeEndorseUserAsync(await Context.Guild.GetUserAsync(id) as SocketGuildUser);

        [Command("clearendorse"), Alias("clearstar", "clearstars")]
        [Summary("Remove all stars from a user.")]
        [Remarks("clearendorse <user>")]
        [RequireModerator]
        public async Task WipeEndorseUserAsync(
            [Summary("The user to remove all endorsement.")] SocketGuildUser guildUser) 
        {
            UserData user = Data.UserData.GetOrCreate(guildUser.Id);

            user.Stars = 0;

            await new EmbedBuilder()
                .WithAuthor(guildUser)
                .WithColor(Color.Green)
                .WithDescription($"Removed all endorsements from {guildUser.Mention}!")
                .Build()
                .SendToChannel(Context.Channel);
        }
    }
}
