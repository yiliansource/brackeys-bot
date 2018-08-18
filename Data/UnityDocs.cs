using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BrackeysBot
{
    /// <summary>
    /// Provides access to a pre-generated lookup of the Unity Docs.
    /// </summary>
    public class UnityDocs
    {
        public List<UnityEntry> ManualEntries { get; set; } = new List<UnityEntry> ();
        public List<UnityEntry> ScriptReferenceEntries { get; set; } = new List<UnityEntry> ();

        public UnityDocs (string manualJsonPath, string scriptReferenceJsonPath)
        {
            string manualEntriesJson = File.ReadAllText(manualJsonPath);
            string scriptReferenceEntriesJson = File.ReadAllText(scriptReferenceJsonPath);

            ManualEntries = JsonConvert.DeserializeObject<List<UnityEntry>> (manualEntriesJson);
            ScriptReferenceEntries = JsonConvert.DeserializeObject<List<UnityEntry>> (scriptReferenceEntriesJson);
        }
    }
}