using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Commands;

namespace BrackeysBot
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireModeratorAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            ulong moderatorRoleId = (context as BrackeysBotContext).Configuration.ModeratorRoleID;
            var guildUser = context.User as IGuildUser;
            bool isModerator = guildUser.RoleIds.Contains(moderatorRoleId) || guildUser.GuildPermissions.Has(GuildPermission.Administrator);

            return Task.FromResult(isModerator
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("User is not a moderator."));
        }
    }
}
