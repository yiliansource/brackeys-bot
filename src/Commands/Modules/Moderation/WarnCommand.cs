using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.Core.Models;
using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public partial class ModerationModule : BrackeysBotModule
    {
        [Command("warn")]
        [Summary("Warns a user with a specified reason.")]
        [Remarks("warn <user> <reason>")]
        [RequireHelper]
        public async Task WarnUserAsync(
            [Summary("The user to warn.")] GuildUserProxy user,
            [Summary("The reason to warn the user."), Remainder] string reason)
        {
            ulong userId = user.HasValue ? user.GuildUser.Id : user.ID;
            UserData data = Data.UserData.GetUser(userId);

            EmbedBuilder builder = new EmbedBuilder()
                .WithColor(Color.Orange);

            bool printInfractions = data != null && data.Infractions?.Count > 0;
            string previousInfractions = null;
            
            // Collect the previous infractions before applying new ones, otherwise we will also collect this
            //  new infraction when printing them
            if (printInfractions)
            {
                previousInfractions = string.Join('\n', data.Infractions.OrderByDescending(i => i.Time).Select(i => i.ToString()));
            }

            Infraction infr = Infraction.Create(Moderation.RequestInfractionID())
                .WithType(InfractionType.Warning)
                .WithModerator(Context.User)
                .WithDescription(reason);

            if (user.HasValue)
                Moderation.AddInfraction(user.GuildUser, infr);
            else 
                Moderation.AddInfraction(user.ID, infr);

            await ModerationLog.CreateEntry(ModerationLogEntry.New
                .WithInfractionId(infr.ID)
                .WithDefaultsFromContext(Context)
                .WithActionType(ModerationActionType.Warn)
                .WithReason(reason)
                .WithTarget(user)
                .WithAdditionalInfo(previousInfractions), Context.Channel);
        }
    }
}
