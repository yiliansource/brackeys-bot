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

        protected const string FILETYPE = "json";
        protected const string TEMPLATE_IDENTIFIER = "template-";

        protected Dictionary<TKey, TValue> Table => _lookup;
        private Dictionary<TKey, TValue> _lookup;

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

        /// <summary>
        /// Adds an element to the table.
        /// </summary>
        public virtual void Add (TKey key, TValue value)
        {
            _lookup.Add(key, value);
            SaveData();
        }
        /// <summary>
        /// Retrieves an element from the table by its key.
        /// </summary>
        public virtual TValue Get(TKey key)
        {
            return _lookup[key];
        }
        /// <summary>
        /// Retrieves an element from the table by its key, or returns null if the element does not exist.
        /// </summary>
        public virtual TValue GetOrDefault(TKey key)
        {
            if (Has(key)) return Get(key);
            else return default;
        }
        /// <summary>
        /// Sets (updates) an element in the table.
        /// </summary>
        public virtual void Set(TKey key, TValue value)
        {
            _lookup[key] = value;
            SaveData();
        }
        /// <summary>
        /// Checks if the table contains the specified key.
        /// </summary>
        public virtual bool Has(TKey key)
        {
            return _lookup.ContainsKey(key);
        }
        /// <summary>
        /// Removes the element with the specified key from the table.
        /// </summary>
        public virtual bool Remove(TKey key)
        {
            bool exists = _lookup.Remove(key);
            SaveData();

            return exists;    
        }
        /// <summary>
        /// Clears all elements from the table.
        /// </summary>
        public virtual void Clear()
        {
            _lookup.Clear();
            SaveData();
        }

        /// <summary>
        /// Saves the lookup data to the disk.
        /// </summary>
        protected virtual void SaveData()
        {
            string contents = JsonConvert.SerializeObject(_lookup, Formatting.Indented);
            File.WriteAllText(FilePath, contents);

            Log.WriteLine($"{this.GetType().Name} was saved!");
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
