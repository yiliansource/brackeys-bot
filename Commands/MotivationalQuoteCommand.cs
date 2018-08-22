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
    public class QuoteForm
{
    public string quoteText;
    public string quoteAuthor;

    public string senderName;

    public string senderLink;
    public string quoteLink;

}
    public class MotivationCommand : ModuleBase
    {
        [Command ("motivation")]
        [Alias ("sad", "depressed", "quote")]
        [HelpData("motivation", "Fetches a motivational quote.")]
        public async Task Motivation()
        {
            QuoteForm qForm = JsonConvert.DeserializeObject<QuoteForm>(await FetchQuote("http://api.forismatic.com/api/1.0/?method=getQuote&format=json&lang=en"));
            await ReplyAsync($"\"{qForm.quoteText}\"{(string.IsNullOrWhiteSpace(qForm.quoteAuthor) ? "" : $"\n- {qForm.quoteAuthor}")}");
        }

        private async Task<string> FetchQuote (string url)
        {
            string resp = string.Empty;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);

            request.Method = "POST";
            request.Accept = "application/json";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse) (await request.GetResponseAsync ()))
                {
                    using (Stream stream = response.GetResponseStream ())
                    using (StreamReader reader = new StreamReader (stream))
                        resp += await reader.ReadToEndAsync () + "\n";
                }
            }
            catch
            {
                await ReplyAsync ("An error has occurred."); 
            }

            return resp;
        }
    }
}