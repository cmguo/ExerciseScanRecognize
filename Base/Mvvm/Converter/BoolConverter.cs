using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class BoolConverter : BooleanConverter<bool>
    {
        public BoolConverter()
        {
            TrueValue = true;
            FalseValue = false;
        }
    }
}
