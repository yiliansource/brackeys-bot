using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public partial class ModerationModule : BrackeysBotModule
    {
        [Command("warn")]
        [Summary("Warns a user with a specified reason.")]
        [Remarks("warn <user> <reason>")]
        [RequireModerator]
        public async Task WarnUserAsync(
            [Summary("The user to warn.")] SocketGuildUser user,
            [Summary("The reason to warn the user."), Remainder] string reason)
        {
            await ReplyAsync($"Alright, I warned {user} for **{reason}**.");

            Moderation.AddInfraction(user, Infraction.Create(Moderation.RequestInfractionID())
                .WithType(InfractionType.Warning)
                .WithModerator(Context.User)
                .WithDescription(reason));
            ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Warn)
                .WithReason(reason)
                .WithTarget(user));
        }
    }
}
