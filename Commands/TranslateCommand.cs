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
    public class TranslationForm
{
    public int code;
    public string lang;
    public string[] text;
}
    public class TranslateCommand : ModuleBase
    {
        private readonly SettingsTable _settingsTable;
        public TranslateCommand(SettingsTable settingsTable)
        {
            _settingsTable = settingsTable;
        }

        [Command ("translate")]
        [HelpData("translate <desired language> <text>", "Translates the given text into the desired language. The language is specified in 2-character format.")]
        public async Task Translate(string lang, [Remainder]string text)
        {
            if (string.IsNullOrWhiteSpace(_settingsTable["yandexkey"]))
            {
                throw new Exception("API key not defined.");
            }
            TranslationForm form = JsonConvert.DeserializeObject<TranslationForm>(await FetchTranslation($"https://translate.yandex.net/api/v1.5/tr.json/translate?key={(_settingsTable["yandexkey"])}&lang={lang}&text={HttpUtility.UrlEncode(text)}"));
            await ReplyAsync("Your translated message:\n" + form.text[0]);
            int i = 1;
            while (i < form.text.Length)
            {
                await ReplyAsync(form.text[i]);
                i++;
            }
            await ReplyAsync("\nPowered by Yandex.Translate");
        }

        private async Task<string> FetchTranslation (string url)
        {
            string resp = string.Empty;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);

            request.Method = "POST";
            request.Accept = "application/x-www-form-urlencoded";

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