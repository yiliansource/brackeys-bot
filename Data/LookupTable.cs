using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrackeysBot
{
    public abstract class LookupTable<T>
    {
        protected abstract T Lookup { get; set; }

        public LookupTable()
        {
            EnsureStorageFile();

            string json = File.ReadAllText(GetFilePath());
            Lookup = JsonConvert.DeserializeObject<T>(json);
        }

        protected virtual void SaveData()
        {
            string contents = JsonConvert.SerializeObject(Lookup);
            File.WriteAllText(GetFilePath(), contents);
        }
        protected virtual void EnsureStorageFile()
        {
            string path = GetFilePath();
            if (!File.Exists(path))
                File.WriteAllText(path, "{}");
        }

        protected abstract string GetFilePath();
    }
}
