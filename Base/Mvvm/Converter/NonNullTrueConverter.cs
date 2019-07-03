using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
