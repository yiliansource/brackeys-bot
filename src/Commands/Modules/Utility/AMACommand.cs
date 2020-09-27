using System.IO;
using System.Threading.Tasks;

using Discord.Commands;

using BrackeysBot.Services;
using Discord;

namespace BrackeysBot.Commands
{
    public partial class UtilityModule : BrackeysBotModule
    {

        [Command("ama")]
        [Summary("Bumps the specified question as an AMA question. Note the question must be in the AMA channel.")]
        [RequireAdministrator]
        [HideFromHelp]
        public async Task Ama(ulong msgId)
        {
            ulong channelId = Data.Configuration.AmaChannelID;

            ITextChannel amaChannel = await Context.Guild.GetTextChannelAsync(channelId);
            IMessage amaMsg = await amaChannel.GetMessageAsync(msgId);

            await Context.Message.DeleteAsync();

            await new EmbedBuilder()
                    .WithAuthor(amaMsg.Author)
                    .WithUrl(amaMsg.GetJumpUrl())
                    .WithColor(Color.Purple)
                    .WithDescription(amaMsg.Content)
                    .Build()
                    .SendToChannel(Context.Channel);

        }
    }
}