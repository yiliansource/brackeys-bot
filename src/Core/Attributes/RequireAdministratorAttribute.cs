using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireAdministratorAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            bool isAdmin = (context.User as IGuildUser).GuildPermissions.Has(GuildPermission.Administrator);

            return Task.FromResult(isAdmin
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("User is not an administrator."));
        }
    }
}
