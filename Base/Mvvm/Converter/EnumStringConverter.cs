using System;
using System.Windows.Data;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(string))]
    public class EnumStringConverter : EnumConverter<string>
    {
    }
}
