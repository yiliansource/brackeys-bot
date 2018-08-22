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
    public class DogPicForm
    {
        public string status;
        public string message;
    }

    public class DogPicCommand : ModuleBase
    {
        [Command("dogpic")]
        [Alias("cute", "eyebleach")]
        public async Task DogPic()
        {
            DogPicForm form = JsonConvert.DeserializeObject<DogPicForm>(await FetchPic("https://dog.ceo/api/breeds/image/random"));
            var eb = new EmbedBuilder();
            eb.WithDescription("Here's your dog picture.");
            eb.WithImageUrl(form.message);
            await ReplyAsync("", false, eb);
        }

        private async Task<string> FetchPic(string url)
        {
            string resp = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        resp += await reader.ReadToEndAsync() + "\n";
                }
            }
            catch
            {
                await ReplyAsync("An error has occurred.");
            }

            return resp;
        }
    }
}