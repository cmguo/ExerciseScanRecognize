using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service
{
    public class ContentSerializer : Attribute
    {
        public Type Value { get; private set; }

        public ContentSerializer(Type type)
        {
            Value = type;
        }

    }
}
