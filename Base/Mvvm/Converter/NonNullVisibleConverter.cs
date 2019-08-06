using System.Windows.Controls;

namespace Base.Mvvm.Converter
{
    public class NonNullVisibleConverter : VisibilityConverter
    {
        public NonNullVisibleConverter()
        {
            CollapsedValues = new object[] { null, false, 0, "" };
        }

    }
}

