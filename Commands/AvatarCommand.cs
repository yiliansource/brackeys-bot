using System.Web;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

using Discord;
using Discord.Commands;


namespace BrackeysBot.Commands
{
    public class AvatarCommand : ModuleBase
    {
        [Command("avatar")]
        [HelpData("avatar <user>", "Generates a link to a user's avatar.")]
        public async Task Avatar(IUser user)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithImageUrl(user.GetAvatarUrl());
            await ReplyAsync("", false, eb);
        }
    }
}