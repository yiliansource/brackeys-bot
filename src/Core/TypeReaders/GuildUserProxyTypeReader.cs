using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Core.Models;

namespace BrackeysBot
{
    public class GuildUserProxyTypeReader : UserTypeReader<IGuildUser>
    {
        // Found this value on https://www.pixelatomy.com/snow-stamp/ -> Enter snowflake of 1, copy UNIX value.
        private readonly int DISCORD_START_UNIX = 1420070400;

        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            // Try to get the result from the base user reader.
            // If that fails, try to solely parse the ID.

            TypeReaderResult result = await base.ReadAsync(context, input, services);
            if (result.IsSuccess)
            {
                IGuildUser user = result.BestMatch as IGuildUser;
                GuildUserProxy proxy = new GuildUserProxy
                {
                    GuildUser = user,
                    ID = user.Id
                };
                return TypeReaderResult.FromSuccess(proxy);
            }
            else
            {
                bool validSnowFlake = false;
                ulong userId = 0;

                if (ulong.TryParse(input, out userId)) 
    	            validSnowFlake = SnowflakeUtils.FromSnowflake(userId).ToUnixTimeSeconds() > DISCORD_START_UNIX;

                if (validSnowFlake || MentionUtils.TryParseUser(input, out userId))
                    return TypeReaderResult.FromSuccess(new GuildUserProxy { ID = userId });
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found.");
        }
    }
}
