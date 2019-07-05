using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Exercise.View
{

    [ValueConversion(typeof(object), typeof(Brush))]
    public class SelectionConverter : IMultiValueConverter
    {

        public Brush Selected { get; set; }
        public Brush Unselected { get; set; }

        public SelectionConverter()
        {
            Selected = Brushes.Blue;
            Unselected = Brushes.Black;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == values[1])
                return Selected;
            else
                return Unselected;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
