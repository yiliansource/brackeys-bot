using System.Threading.Tasks;
using System.Web;

using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class LmgtfyCommand : ModuleBase
    {
        [Command("lmgtfy")]
        [HelpData("lmgtfy <search>", "Performs a google search for a user that doesn't know how to use google.")]
        public async Task GoogleCommand([Remainder] string search)
            => await ReplyAsync($"http://google.com/search?q={HttpUtility.UrlEncode(search)}");
    }
}
