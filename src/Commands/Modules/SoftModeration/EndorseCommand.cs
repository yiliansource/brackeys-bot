using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Services;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;

namespace BrackeysBot.Commands
{
    public partial class SoftModerationModule : BrackeysBotModule
    {
        public EndorseService Endorsements { get; set; }

        [Command("endorse"), Alias("star", "stars")]
        [Summary("Endorse a user and give them a star.")]
        [Remarks("endorse <user>")]
        [RequireGuru]
        public async Task EndorseUserAsync(
            [Summary("The user to endorse.")] SocketGuildUser guildUser) 
        {
            int newAmount = Endorsements.GetUserStars(guildUser) + 1;

            Endorsements.SetUserStars(guildUser, newAmount);

            await new EmbedBuilder()
                .WithAuthor(guildUser)
                .WithDescription($"Gave a :star: to {guildUser.Mention}! They now have {newAmount} stars!")
                .WithColor(Color.Gold)
                .Build()
                .SendToChannel(Context.Channel);
        }

        [Command("endorse"), Alias("star", "stars")]
        [Summary("Display your stars")]
        [Remarks("endorse")]
        public async Task EndorseUserAsync() {
            int amount = Endorsements.GetUserStars(Context.Message.Author);

            await new EmbedBuilder()
                .WithColor(Color.Gold)
                .WithDescription($"You have {amount} :star:")
                .Build()
                .SendToChannel(Context.Channel);
        }
        

        [Command("deleteendorse"), Alias("deletestar", "delstar", "delendorse")]
        [Summary("Remove a star from a user.")]
        [Remarks("deleteendorse <user>")]
        [RequireModerator]
        public async Task DeleteEndorseUserAsync(
            [Summary("The user to remove an endorsement.")] SocketGuildUser guildUser) 
        {
            int currentAmount = Endorsements.GetUserStars(guildUser);

            EmbedBuilder builder = new EmbedBuilder();

            if (currentAmount == 0) 
            {
                builder.WithColor(Color.Red).WithDescription("Can't remove a star, they have none!");
            } 
            else 
            {
                int newAmount = currentAmount - 1;
                Endorsements.SetUserStars(guildUser, newAmount);

                builder.WithAuthor(guildUser).WithColor(Color.Green)
                    .WithDescription($"Took a :star: from {guildUser.Mention}! They now have {newAmount} stars!");
            }

            await builder.Build()
                .SendToChannel(Context.Channel);
        }

        [Command("clearendorse"), Alias("clearstar", "clearstars")]
        [Summary("Remove all stars from a user.")]
        [Remarks("clearendorse <user>")]
        [RequireModerator]
        public async Task WipeEndorseUserAsync(
            [Summary("The user to remove all endorsement.")] SocketGuildUser guildUser) 
        {
            Endorsements.SetUserStars(guildUser, 0);

            await new EmbedBuilder()
                .WithAuthor(guildUser)
                .WithColor(Color.Green)
                .WithDescription($"Removed all endorsements from {guildUser.Mention}!")
                .Build()
                .SendToChannel(Context.Channel);
        }

        [Command("setendorse"), Alias("setstar", "setstars")]
        [Summary("Set the stars of a user.")]
        [Remarks("setendorse <user> <amount>")]
        [RequireModerator]
        public async Task SetEndorseUserAsync(
            [Summary("The user to set the endorsement.")] SocketGuildUser guildUser,
            [Summary("The amount of endorsement to set.")] int amount) 
        {
            Endorsements.SetUserStars(guildUser, amount);

            await new EmbedBuilder()
                .WithAuthor(guildUser)
                .WithColor(Color.Green)
                .WithDescription($"Set the endorsements of {guildUser.Mention} to {amount}:star:!")
                .Build()
                .SendToChannel(Context.Channel);
        }

        [Command("topendorse"), Alias("topstar", "topstars")]
        [Summary("Show the users with most stars.")]
        [Remarks("topendorse")]
        [RequireGuru]
        public async Task TopEndorseAsync() 
        {
            await GetDefaultBuilder()
                .WithTitle("Endorse Leaderboard")
                .WithFields(Endorsements.GetEndorseLeaderboard()
                    .Select((l, i) => new EmbedFieldBuilder()
                        .WithName((i + 1).ToString().Envelop("**"))
                        .WithValue($"{l.User.Mention} · {l.Stars} :star:")
                        .WithIsInline(true)))
                .Build()
                .SendToChannel(Context.Channel);
        }
    }
}
