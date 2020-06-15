using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.Models.Database;
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
            await Infractions.AddInfraction(Context.Channel, Infractions.CreateInfraction(user, Context.User, InfractionType.Warning, null, reason));
        }
    }
}
