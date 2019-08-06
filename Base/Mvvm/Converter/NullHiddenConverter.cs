namespace Base.Mvvm.Converter
{
    public class NullHiddenConverter : VisibilityConverter
    {
        public NullHiddenConverter()
        {
            HiddenValues = new object[] { null, "" };
        }

    }
}

