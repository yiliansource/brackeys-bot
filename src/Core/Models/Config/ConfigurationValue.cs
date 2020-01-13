using System;
using System.Reflection;
using System.ComponentModel;

using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class ConfigurationValue
    {
        public string Name { get; }
        public string Description { get; }
        public bool Confidential { get; }

        public object Value
        {
            get
            {
                return Confidential
                    ? null
                    : _property.GetValue(_instance);
            }
        }

        private PropertyInfo _property;
        private object _instance;

        public ConfigurationValue(PropertyInfo property, object instance) 
        {
            Name = property.GetCustomAttribute<YamlMemberAttribute>()?.Alias ?? property.Name;
            Description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
            Confidential = property.GetCustomAttribute<ConfidentialAttribute>() != null;

            _property = property;
            _instance = instance;
        }

        public bool SetValue(object value)
        {
            Type source = value.GetType();
            Type target = _property.PropertyType;

            object convertedValue = value;
            if (source != target)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(target);

                if (converter != null && converter.CanConvertFrom(source) && converter.IsValid(value))
                    convertedValue = converter.ConvertFrom(value);
                else 
                    return false;
            }

            _property.SetValue(_instance, convertedValue);
            return true;
        }
    }
}
