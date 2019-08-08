using System;
using System.Globalization;
using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    public class BinaryConverter<T> : IValueConverter
    {

        public T TrueValue { get; set; }

        public T FalseValue { get; set; }

        public object[] TrueValues { get; set; }
        public object[] FalseValues { get; set; }

        public BinaryConverter()
        {
            FalseValues = new object[] { 0, false, "", null };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TrueValues != null && Array.IndexOf(TrueValues, value) >= 0)
                return TrueValue;
            else if (FalseValues != null && Array.IndexOf(FalseValues, value) >= 0)
                return FalseValue;
            else if (TrueValues == null)
                return TrueValue;
            else if (FalseValues == null)
                return FalseValue;
            else
                return TrueValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
