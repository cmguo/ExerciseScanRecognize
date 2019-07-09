namespace Base.Mvvm.Converter
{
    public class NonNullTrueConverter : BoolConverter
    {
        public NonNullTrueConverter()
        {
            FalseValues = new object[] { null, "" };
        }
    }
    
}
