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

        private static readonly HashSet<Type> _exposedPropertyTypes
            = new HashSet<Type>() { typeof(string), typeof(bool), 
                typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong),
                typeof(float), typeof(double) };
        
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
         => GetMainConfigurationValues().Concat(GetSubConfigurationValues());

        private IEnumerable<ConfigurationValue> GetMainConfigurationValues()
            => GetConfigProperties().Select(p => new ConfigurationValue(p, _config));

        private IEnumerable<PropertyInfo> GetConfigProperties()
            => GetConfigProperties(typeof(BotConfiguration));

        private IEnumerable<PropertyInfo> GetConfigProperties(Type type)
        => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(p =>
                {
                    bool isValidType = _exposedPropertyTypes.Contains(p.PropertyType)
                        || p.PropertyType.IsArray && _exposedPropertyTypes.Contains(p.PropertyType.GetElementType());
                    bool confidential = p.GetCustomAttribute<ConfidentialAttribute>() != null;

                    return isValidType && !confidential;
                });

        private IEnumerable<ConfigurationValue> GetSubConfigurationValues() 
            => typeof(BotConfiguration).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(p => p.GetCustomAttribute<ConfigSubObjectAttribute>() != null)
                .SelectMany(p => {
                    object subConfig = p.GetValue(_config);
                    if (subConfig == null)
                        return new ConfigurationValue[0];
                    string subPropertyNamePrefix = $"{ConfigurationValue.GetName(p)}.";
                    return GetConfigProperties(subConfig.GetType()).Select(pSubConfig => new ConfigurationValue(pSubConfig, subConfig, subPropertyNamePrefix));
                });

        public void Save()
            => _data.SaveConfiguration();

        

        
    }
}
