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
        public object[] CollapsedValues { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (VisibleValues != null && Array.IndexOf(VisibleValues, value) >= 0)
                return Visibility.Visible;
            else if (HiddenValues != null && Array.IndexOf(HiddenValues, value) >= 0)
                return Visibility.Hidden;
            else if (CollapsedValues != null && Array.IndexOf(CollapsedValues, value) >= 0)
                return Visibility.Collapsed;
            else if (VisibleValues == null)
                return Visibility.Visible;
            else if (HiddenValues == null)
                return Visibility.Hidden;
            else if (CollapsedValues == null)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
