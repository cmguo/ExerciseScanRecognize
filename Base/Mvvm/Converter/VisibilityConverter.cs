using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class VisibilityConverter : IValueConverter
    {
            
        public object[] VisibleValues { get; set; }
        public object[] HiddenValues { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Array.IndexOf(VisibleValues, value) >= 0)
                return Visibility.Visible;
            else if (HiddenValues != null && Array.IndexOf(HiddenValues, value) >= 0)
                return Visibility.Hidden;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
