using Discord.WebSocket;
using System.Threading.Tasks;
using System.Web;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class LmgtfyCommand : ModuleBase
    {
        public LmgtfyCommand()
        {
        }

        [Command("lmgtfy")]
        [HelpData("lmgtfy <search>", "Performs a google search for a user that doesn't know how to use google")]
        public async Task GoogleCommand([Remainder] string search)
        {
            await ReplyAsync(SearchBuilder(search));
            return;
        }
       
        private static string SearchBuilder(string search)
        {
            string url = $"http://lmgtfy.com/?q={HttpUtility.UrlEncode(search)}";
            return url;
        }


    }
}
