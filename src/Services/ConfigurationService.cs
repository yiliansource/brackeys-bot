using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace BrackeysBot.Services
{
    public sealed class ConfigurationService : BrackeysBotService
    {
        private DataService _data;
        private BotConfiguration _config;
        
        public ConfigurationService(DataService data)
        {
            _data = data;
            _config = data.Configuration;
        }

        public bool TryGetValue(string name, out ConfigurationValue value)
        {
            value = GetConfigurationValues().FirstOrDefault(v => v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return value != null;
        }
        public IEnumerable<ConfigurationValue> GetConfigurationValues()
            => GetConfigProperties().Select(p => new ConfigurationValue(p, _config));
        public void Save()
            => _data.SaveConfiguration();

        private IEnumerable<PropertyInfo> GetConfigProperties()
            => typeof(BotConfiguration).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(p => typeof(IConvertible).IsAssignableFrom(p.PropertyType));
    }
}
