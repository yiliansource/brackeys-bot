using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Commands
{
    public class UnityCommands : ModuleBase
    {
        private const int MaxNumberOfEntries = 4;

        private UnityDocs _docs;

        public UnityCommands (UnityDocs docs)
        {
            _docs = docs;
        }

        [Command ("manual")]
        [HelpData("manual <search>", "Retrieves information from the Unity Manual.")]
        public async Task UnityManual ([Remainder] string search)
        {
            await Search (_docs.ManualEntries, search);
        }

        [Command ("scriptapi")]
        [HelpData("scriptapi <search>", "Retrieves information from the Unity Scripting API Reference.")]
        public async Task UnityScriptingReference ([Remainder] string search)
        {
            await Search (_docs.ScriptReferenceEntries, search);
        }

        private async Task Search (List<UnityEntry> entries, string search)
        {
            // Should the results display more than one result?
            string [] words = search.Split (" ");
            bool displayMoreThanOne = words [0] == "-m";
            // Remove the parameter from the search string
            if (displayMoreThanOne)
                search = search.Remove (0, 3);

            EmbedBuilder eb = new EmbedBuilder ();

            eb.Title = "Search results:";

            if (displayMoreThanOne)
            {
                var query = from e in entries
                            where (e.Title.ToLower ().Contains (search.ToLower ()))
                            select e;

                for (int i = 0; i < MaxNumberOfEntries; i++)
                {
                    if (i + 1 > query.Count ())
                        break;
                    UnityEntry e = query.ElementAt (i);
                    eb.AddField (e.Title, $"[⏎]({e.Link}) {e.Description}");
                }
            }
            else
            {
                var query = from e in entries
                            where (e.Title.ToLower () == search.ToLower ())
                            select e;

                if (query.Count () == 0)
                    query = from e in entries
                            where (e.Title.ToLower ().Contains (search.ToLower()))
                            select e;

                if (query.Count () == 0)
                {
                    eb.Title = "Nothing found";
                    eb.Color = Color.Red;
                }
                else
                {
                    UnityEntry e = query.First ();
                    eb.Title = e.Title;
                    eb.Url = e.Link;
                    eb.Description = e.Description;
                }
            }

            await ReplyAsync ("", false, eb.Build());
        }
    }
}
