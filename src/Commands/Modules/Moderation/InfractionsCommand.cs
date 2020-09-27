using System;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.Core.Models;
using Discord;
using Discord.Commands;

using Humanizer;

namespace BrackeysBot.Commands
{
    public partial class ModerationModule : BrackeysBotModule
    {
        [Command("infractions"), Alias("infraction", "infr")]
        [Summary("Displays all the infractions of a user.")]
        [Remarks("infractions <user>")]
        [RequireModerator]
        public async Task GetInfractionsAsync(
            [Summary("The user to get the infractions for.")] GuildUserProxy user)
        {
            ulong userId = user.HasValue ? user.GuildUser.Id : user.ID;
            UserData data = Data.UserData.GetUser(userId);

            EmbedBuilder builder = new EmbedBuilder()
                .WithColor(Color.DarkerGrey);

            if (data != null && data.Infractions?.Count > 0)
            {
                if (user.HasValue)
                    builder.WithAuthor($"{user.ID.Mention()} has {data.Infractions.Count} infraction(s)", user.GuildUser.EnsureAvatarUrl());
                else 
                    builder.WithAuthor($"{user.ID} has {data.Infractions.Count} infraction(s)");
                    
                builder.WithDescription(string.Join('\n', data.Infractions.OrderByDescending(i => i.Time).Select(i => i.ToString())));
            }
            else
            {
                if (user.HasValue)
                    builder.WithAuthor($"{user.ID.Mention()} has no infractions", user.GuildUser.EnsureAvatarUrl());
                else 
                    builder.WithAuthor($"{user.ID} has no infractions");
            }

            await builder.Build().SendToChannel(Context.Channel);
        }

        [Command("clearinfractions"), Alias("clearinfr")]
        [Summary("Clears the infractions of a user.")]
        [Remarks("clearinfractions <user>")]
        [RequireModerator]
        public async Task ClearInfractionsAsync(
            [Summary("The user to clear the infractions from.")] GuildUserProxy user)
        {
            ulong userId = user.HasValue ? user.GuildUser.Id : user.ID;

            int clearedInfractions = Moderation.ClearInfractions(userId);

            EmbedBuilder builder = GetDefaultBuilder();

            if (user.HasValue) 
                builder.WithDescription($"{clearedInfractions} infraction(s) were cleared from {userId.Mention()}.");
            else 
                builder.WithDescription($"{clearedInfractions} infraction(s) were cleared from {userId}.");
                
            await builder.Build()
                .SendToChannel(Context.Channel);

            if (clearedInfractions > 0)
            {
                await ModerationLog.CreateEntry(ModerationLogEntry.New
                    .WithDefaultsFromContext(Context)
                    .WithActionType(ModerationActionType.ClearInfractions)
                    .WithTarget(userId));
            }
        }

        [Command("infraction"), Alias("infr")]
        [Summary("Displays more information about an infraction")]
        [Remarks("infraction <id>")]
        [RequireModerator]
        public async Task ShowInfractionAsync(
            [Summary("The ID of the infraction.")] int id)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"Infraction {id}");

            if (Moderation.TryGetInfraction(id, out Infraction infraction, out ulong userId))
            {
                builder.WithFooter($"Infraction added {infraction.Time.Humanize()}.")
                    .WithColor(Color.Orange)
                    .AddField("User", userId.Mention(), true)
                    .AddField("Type", infraction.Type.Humanize(), true)
                    .AddField("Moderator", infraction.Moderator.Mention(), true)
                    .AddField("Description", infraction.Description, true)
                    .AddFieldConditional(!string.IsNullOrEmpty(infraction.AdditionalInfo), "Additional Information", infraction.AdditionalInfo, true);
            }
            else
            {
                builder.WithDescription("Infraction does not exist.")
                    .WithColor(Color.Red);
            }

            await builder.Build().SendToChannel(Context.Channel);
        }

        [Command("deleteinfraction"), Alias("delinfraction", "deleteinfr", "delinfr")]
        [Summary("Deletes an infraction by its ID.")]
        [Remarks("deleteinfraction <id>")]
        [RequireModerator]
        public async Task DeleteInfractionAsync(
            [Summary("The ID of the infraction")] int id)
        {
            if (!Moderation.TryGetInfraction(id, out Infraction _, out ulong userId))
                throw new ArgumentException($"An infraction with the ID of **{id}** does not exist.");

            Moderation.DeleteInfraction(id);

            await GetDefaultBuilder()
                .WithDescription($"The infraction with the ID **{id}** was deleted from {MentionUtils.MentionUser(userId)}.")
                .Build()
                .SendToChannel(Context.Channel);

            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.DeletedInfraction)
                .WithTarget(userId));
        }

        [Command("editinfraction"), Alias("editinfr")]
        [Summary("Edit an infraction by its ID.")]
        [Remarks("editinfraction <id> <new message>")]
        [RequireModerator]
        public async Task EditInfractionAsync(
            [Summary("The ID of the infraction")] int id,
            [Summary("The new infraction message"), Remainder] string newMessage)
        {
            if (Moderation.TryUpdateInfraction(id, newMessage, out ulong userId, out string oldMessage)) 
            {
                await GetDefaultBuilder()
                    .WithColor(Color.Green)
                    .WithDescription($"The infraction with the ID **{id}** was updated from {MentionUtils.MentionUser(userId)}.")
                    .Build()
                    .SendToChannel(Context.Channel);

                await ModerationLog.CreateEntry(ModerationLogEntry.New
                    .WithDefaultsFromContext(Context)
                    .WithActionType(ModerationActionType.UpdatedInfraction)
                    .WithAdditionalInfo($"**Old Reason:** {oldMessage}\n**New Reason:** {newMessage}")
                    .WithTarget(userId));
            } else 
                throw new ArgumentException($"An infraction with the ID of **{id}** does not exist.");
        }
    }
}
