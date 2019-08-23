using System.Configuration;

namespace Base.Config
{

    public class SimpleElement
    {
        public static object Unwrap(object e)
        {
            if (e is ISimpleElement se)
                return se.Value;
            return e;
        }
    }

    public interface ISimpleElement
    {
        object Value { get; set; }
    }

    public class SimpleElement<T> : ConfigurationElement, ISimpleElement
    {
        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty propValue;

        static SimpleElement()
        {
            propValue = new ConfigurationProperty("Value", typeof(T),
                null, ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propValue);
        }

        protected override ConfigurationPropertyCollection Properties => properties;

        [ConfigurationProperty("Value", IsRequired = true)]
        public T Value
        {
            get => base[propValue] == null ? default(T) : (T)base[propValue];
            set => base[propValue] = value;
        }

        object ISimpleElement.Value
        {
            get => Value;
            set => Value = value == null ? default(T) : (T)value;
        }

    }
}
