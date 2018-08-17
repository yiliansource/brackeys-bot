using System.Collections.Generic;
using Newtonsoft.Json;

namespace BrackeysBot
{
    public class UnityDocs
    {
        public List<UnityEntry> ManualEntries { get; set; } = new List<UnityEntry> ();
        public List<UnityEntry> ScriptReferenceEntries { get; set; } = new List<UnityEntry> ();

        public UnityDocs (string manualJson, string scriptReferenceJson)
        {
            ManualEntries = JsonConvert.DeserializeObject<List<UnityEntry>> (manualJson);
            ScriptReferenceEntries = JsonConvert.DeserializeObject<List<UnityEntry>> (scriptReferenceJson);
        }
    }
}