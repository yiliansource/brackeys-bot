using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;

using YamlDotNet.Serialization;

namespace BrackeysBot.Services
{
    public class DataService : BrackeysBotService
    {
        public BotConfiguration Configuration => _configuration;

        private BotConfiguration _configuration;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = true
        };

        private const string _databasePath = "users.json";
        private const string _configPath = "config.yaml";

        public DataService()
        {
            LoadConfiguration();
        }

        public void SaveConfiguration()
        {
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve).Build();
            string yaml = serializer.Serialize(_configuration);
            File.WriteAllText(_configPath, yaml);
        }
        public void LoadConfiguration()
        {
            if (!File.Exists(_configPath))
            {
                _configuration = new BotConfiguration();
                SaveConfiguration();
                return;
            }

            var deserializer = new DeserializerBuilder().Build();
            _configuration = deserializer.Deserialize<BotConfiguration>(File.ReadAllText(_configPath));
        }
    }
}
