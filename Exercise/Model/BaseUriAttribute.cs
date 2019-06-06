using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Model
{
    class BaseUriAttribute : Attribute
    {

        public string Value { get; private set; }

        public BaseUriAttribute(string path)
        {
            Value = path;
        }

    }
}
