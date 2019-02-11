using System.Collections.Generic;

using Newtonsoft.Json;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a lookup table that will be serialized into a JSON file.
    /// </summary>
    public abstract class LookupTable<TKey, TValue> : DataFile, ILookupTable<TKey, TValue>
    {
        protected Dictionary<TKey, TValue> Table => _lookup;
        private Dictionary<TKey, TValue> _lookup;

        public LookupTable()
        {
            LoadData();
        }

        /// <summary>
        /// Serializes the contents of the lookup table to a string.
        /// </summary>
        protected override string SaveToString()
            => JsonConvert.SerializeObject(_lookup, Formatting.Indented);
        /// <summary>
        /// Deserializes the contents of the lookup table from a string.
        /// </summary>
        protected override void LoadFromString(string value)
            => _lookup = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(value);
        
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
            else return default(TValue);
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
            bool exists = _lookup.Remove(key);
            SaveData();
            return exists;    
        }
    }
}
