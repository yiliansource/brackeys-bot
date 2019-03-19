using System.IO;

namespace BrackeysBot
{
    /// <summary>
    /// Represents a persistant data file.
    /// </summary>
    public abstract class DataFile
    {
        /// <summary>
        /// Returns the full name of the file, including the extension.
        /// </summary>
        public string FullName => $"{ FileName }.{ FILETYPE }";
        /// <summary>
        /// Returns the name of the data file (excluding the extension).
        /// </summary>
        public abstract string FileName { get; }
        /// <summary>
        /// Determines whether the data file should require a template file.
        /// </summary>
        public virtual bool RequiresTemplateFile => false;

        /// <summary>
        /// Serializes the contents of the file to an object.
        /// </summary>
        protected abstract string SaveToString();
        /// <summary>
        /// Loads the data file from the specified object.
        /// </summary>
        protected abstract void LoadFromString(string value);

        private const string FILETYPE = "json";
        private const string TEMPLATE_IDENTIFIER = "template-";

        /// <summary>
        /// Saves the lookup data to the disk.
        /// </summary>
        protected virtual void SaveData()
        {
            File.WriteAllText(FullName, SaveToString());
            Log.WriteLine($"{this.GetType().Name} was saved!");
        }
        /// <summary>
        /// Loads the lookup data from the disk.
        /// </summary>
        protected virtual void LoadData()
        {
            // Check if the file exists
            if (!File.Exists(FullName))
            {
                if (RequiresTemplateFile)
                {
                    // If the file requires a template, load the template
                    string templatePath = TEMPLATE_IDENTIFIER + FullName;
                    if (File.Exists(templatePath))
                    {
                        string filename = templatePath.Substring(TEMPLATE_IDENTIFIER.Length);
                        File.Copy(templatePath, FullName);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Template file for { GetType().Name } was requested, but not found.");
                    }
                }
                else
                {
                    // If not, ensure an empty storage file
                    EnsureStorageFile();
                }
            }

            string json = File.ReadAllText(FullName);
            LoadFromString(json);
        }
        /// <summary>
        /// Ensures that a lookup file exist at the filepath.
        /// </summary>
        protected virtual void EnsureStorageFile()
        {
            if (!File.Exists(FullName))
                File.WriteAllText(FullName, "{}");
        }
    }
}
