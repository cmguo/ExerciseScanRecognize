using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Exercise.View.Resolve
{

    [ValueConversion(typeof(object), typeof(Brush))]
    public class SelectionConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == values[1])
                return Brushes.Blue;
            else
                return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
