using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class BoolConverter : IValueConverter
    {
            
        public object[] TrueValues { get; set; }
        public object[] FalseValues { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TrueValues != null && Array.IndexOf(TrueValues, value) >= 0)
                return true;
            else if (FalseValues != null && Array.IndexOf(FalseValues, value) >= 0)
                return false;
            else if (TrueValues == null)
                return true;
            else if (FalseValues == null)
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
