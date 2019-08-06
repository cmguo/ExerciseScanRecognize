using System.Windows;
using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(object))]
    public class BooleanVisibilityConverter : BooleanConverter<Visibility>
    {
    }

}
