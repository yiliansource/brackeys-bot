using System;
using System.Threading.Tasks;
using BrackeysBot.Core.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public partial class ModerationModule : BrackeysBotModule
    {
        [Command("kick")]
        [Summary("Kicks a user from the server.")]
        [Remarks("kick <user> <reason>")]
        [RequireModerator]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(
            [Summary("The user to kick.")] GuildUserProxy user,
            [Summary("The reason to kick the user for."), Remainder] string reason = DefaultReason)
        {
            if (!user.HasValue)
                throw new ArgumentException($"User with ID {user.ID} is not in the server!");

            await user.GuildUser.KickAsync(reason);

            Infraction infraction = Infraction.Create(Moderation.RequestInfractionID())
                .WithType(InfractionType.Kick)
                .WithModerator(Context.User)
                .WithDescription(reason);

            if (user.HasValue)
                Moderation.AddInfraction(user.GuildUser, infraction);
            else 
                Moderation.AddInfraction(user.ID, infraction);


            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithInfractionId(infraction.ID)
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Kick)
                .WithReason(reason)
                .WithTarget(user), Context.Channel);
        }
    }
}
