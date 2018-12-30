using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class SayCommand : ModuleBase
    {
        [Command("say")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("say <channel-id> <message>", "Makes the bot say the specified message in the specified channel.")]
        public async Task Say(ulong channel, [Remainder]string message)
        {
            var channels = await Context.Guild.GetChannelsAsync();
            var targetChannel = channels.FirstOrDefault(c => c.Id == channel);

            if (targetChannel == null || !(targetChannel is IMessageChannel))
            {
                await ReplyAsync("The specified channel doesn't exist!");
                return;
            }

            await (targetChannel as IMessageChannel).SendMessageAsync(message);
        }
    }
}