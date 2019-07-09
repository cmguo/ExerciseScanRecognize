namespace Base.Mvvm.Converter
{
    public class NonNullVisibleConverter : VisibilityConverter
    {
        public NonNullVisibleConverter()
        {
            CollapsedValues = new object[] { null, "" };
        }

    }
}

