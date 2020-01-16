using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

using YamlDotNet.Serialization;

namespace BrackeysBot
{
    public class ConfigurationValue
    {
        public string Name { get; }
        public string Description { get; }
        public bool Confidential { get; }

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

        public object GetValue()
            => Confidential ? null : _property.GetValue(_instance);
        public bool SetValue(object value)
        {
            Type source = value.GetType();
            Type target = _property.PropertyType;

            if (target.IsClass && (value == null || (value is string s && s == "null")))
            {
                _property.SetValue(_instance, null);
                return true;
            }

            object convertedValue = value;
            if (source != target)
            {
                if (!target.IsArray)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(target);

                    if (converter != null && converter.CanConvertFrom(source) && converter.IsValid(value))
                        convertedValue = converter.ConvertFrom(value);
                    else
                        return false;
                }
                else
                {
                    if (source == typeof(string))
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(target.GetElementType());

                        if (converter == null || !converter.CanConvertFrom(source))
                            return false;

                        Array convertedValueArray = value.ToString().Split(',')
                            .Select(part => converter.ConvertFrom(part.Trim()))
                            .ToArray();

                        Array typedConvertedArray = Array.CreateInstance(target.GetElementType(), convertedValueArray.Length);
                        Array.Copy(convertedValueArray, typedConvertedArray, convertedValueArray.Length);

                        convertedValue = typedConvertedArray;
                    }
                    else
                        return false;
                }
            }

            _property.SetValue(_instance, convertedValue);
            return true;
        }

        public override string ToString()
        {
            if (Confidential)
                return "[hidden]";

            object value = GetValue();

            if (!_property.PropertyType.IsArray)
                return value.ToString();
            else
                return value == null
                    ? "null"
                    : string.Join(", ", ((IEnumerable)value).OfType<object>());
        }
    }
}
