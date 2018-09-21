using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a lookup table that will be serialized into a JSON file.
    /// </summary>
    public abstract class LookupTable<TKey, TValue> : ILookupTable<TKey, TValue>
    {
        /// <summary>
        /// Returns the path of the lookup file.
        /// </summary>
        public string FilePath => $"{ FileName }.{ FILETYPE }";
        /// <summary>
        /// Returns the name of the lookup file (without the extension).
        /// </summary>
        public abstract string FileName { get; }
        /// <summary>
        /// Determines whether the lookup file requires a template file.
        /// </summary>
        public virtual bool RequiresTemplateFile => false;

        private const string FILETYPE = "json";
        private const string TEMPLATE_IDENTIFIER = "template-";
        
        protected Dictionary<TKey, TValue> _lookup;

        public LookupTable()
        {
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
            return _lookup.Remove(key);
        }

        /// <summary>
        /// Saves the lookup data to the disk.
        /// </summary>
        protected virtual void SaveData()
        {
            string contents = JsonConvert.SerializeObject(_lookup, Formatting.Indented);
            File.WriteAllText(FilePath, contents);
        }
        /// <summary>
        /// Loads the lookup data from the disk.
        /// </summary>
        protected virtual void LoadData()
        {
            // Check if the file exists
            if (!File.Exists(FilePath))
            {
                // If the file requires a template, load the template
                if (RequiresTemplateFile)
                {
                    string templatePath = TEMPLATE_IDENTIFIER + FilePath;
                    if (File.Exists(templatePath))
                    {
                        string filename = templatePath.Substring(TEMPLATE_IDENTIFIER.Length);
                        File.Copy(templatePath, FilePath);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Template file for { GetType().Name } was requested, but not found.");
                    }
                }
                // If not, ensure an empty storage file
                else
                {
                    EnsureStorageFile();
                }
            }

            string json = File.ReadAllText(FilePath);
            _lookup = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
        }
        /// <summary>
        /// Ensures that a lookup file exist at the filepath.
        /// </summary>
        protected virtual void EnsureStorageFile()
        {
            if (!File.Exists(FilePath))
                File.WriteAllText(FilePath, "{}");
        }
    }
}
