using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(object))]
    public class BinaryObjectConverter : BinaryConverter<object>
    {
    }

}
