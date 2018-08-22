using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a lookup table that will be serialized into a JSON file.
    /// </summary>
    public class LookupTable<TKey, TValue> : ILookupTable<TKey, TValue>
    {
        [JsonProperty("Lookup")]
        protected Dictionary<TKey, TValue> _lookup;

        [JsonIgnore]
        protected string _filepath;

        public LookupTable(string path)
        {
            _filepath = path;

            EnsureStorageFile();
            LoadData();
        }

        /// <summary>
        /// Gets or sets a value in the lookup.
        /// </summary>
        public virtual TValue this[TKey index]
        {
            get => _lookup[index];
            set => _lookup[index] = value;
        }

        public virtual void Add (TKey key, TValue value)
        {
            _lookup.Add(key, value);
            SaveData();
        }
        public virtual TValue Get(TKey key)
        {
            return _lookup[key];
        }
        public virtual TValue GetOrDefault(TKey key)
        {
            if (Has(key)) return Get(key);
            else return default;
        }
        public virtual void Set(TKey key, TValue value)
        {
            _lookup[key] = value;
            SaveData();
        }
        public virtual bool Has(TKey key)
        {
            return _lookup.ContainsKey(key);
        }
        public virtual bool Remove(TKey key)
        {
            return _lookup.Remove(key);
        }

        /// <summary>
        /// Saves the lookup data to the disk.
        /// </summary>
        protected virtual void SaveData()
        {
            string contents = JsonConvert.SerializeObject(_lookup);
            File.WriteAllText(_filepath, contents);
        }
        /// <summary>
        /// Loads the lookup data from the disk.
        /// </summary>
        protected virtual void LoadData()
        {
            string json = File.ReadAllText(_filepath);
            _lookup = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
        }
        /// <summary>
        /// Ensures that a lookup file exist at the filepath.
        /// </summary>
        protected virtual void EnsureStorageFile()
        {
            if (!File.Exists(_filepath))
                File.WriteAllText(_filepath, "{}");
        }
    }
}
