namespace Base.Mvvm.Converter
{
    public class NullVisibleConverter : VisibilityConverter
    {
        public NullVisibleConverter()
        {
            VisibleValues = new object[] { null, "" };
        }

    }
}

