using LiteDB;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace BrackeysBot.Services
{
    public class DataService : BrackeysBotService
    {
        public BotConfiguration Configuration => _configuration;

        private LiteDatabase _database = new LiteDatabase(_databasePath);
        private BotConfiguration _configuration;

        private const string _databasePath = "Data/BrackeysBot.db";
        private const string _configPath = "config.yaml";

        public DataService()
        {
            LoadConfiguration();
        }

        public void SaveConfiguration()
        {
            var serializer = new SerializerBuilder().Build();
            File.WriteAllText(_configPath, serializer.Serialize(_configuration));
        }
        public void LoadConfiguration()
        {
            var deserializer = new DeserializerBuilder().Build();
            _configuration = deserializer.Deserialize<BotConfiguration>(File.ReadAllText(_configPath));
        }
    }
}
