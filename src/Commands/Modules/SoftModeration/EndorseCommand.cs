using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using BrackeysBot.Services;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public partial class SoftModerationModule : BrackeysBotModule
    {
        [Command("endorse"), Alias("star")]
        [Summary("Endorse a user and give them a star.")]
        [Remarks("endorse <user>")]
        [RequireModerator]
        public async Task EndorseUserAsync(
            [Summary("The user ID to endorse.")] ulong id)
        {
            Data.UserData.GetOrCreate(id).
            IMessage message = await Context.Channel.GetMessageAsync(id);
            string url = await Codeblock.PasteMessage(message);

            await GetDefaultBuilder()
                .WithAuthor("Pasted!", message.Author.EnsureAvatarUrl())
                .WithDescription($"The message by {message.Author.Mention} has been pasted!\nClick [here]({url}) to view it!")
                .WithColor(Color.Green)
                .Build()
                .SendToChannel(Context.Channel);

            await message.DeleteAsync();
        }

        [Command("endorse"), Alias("star")]
        [Summary("Endorse a user and give them a star.")]
        [Remarks("endorse <id>")]
        [RequireModerator]
        public async Task EndorseUserAsync(
            [Summary("The user to ban.")] SocketGuildUser user) 
            => await EndorseUserAsync(user.Id);
    }
}
