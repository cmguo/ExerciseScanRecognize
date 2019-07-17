using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    public class EnumConverter<T> : IValueConverter
    {

        public Collection<T> Values { get; set; }

        public EnumConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int n = Array.IndexOf(Enum.GetValues(value.GetType()), value);
            if (n >= 0)
                return Values[n];
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
