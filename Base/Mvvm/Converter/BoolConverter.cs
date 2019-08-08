using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class BoolConverter : BinaryConverter<bool>
    {
        public BoolConverter()
        {
            TrueValue = true;
            FalseValue = false;
        }
    }
}
