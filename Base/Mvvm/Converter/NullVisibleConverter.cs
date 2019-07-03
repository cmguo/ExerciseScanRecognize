using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

