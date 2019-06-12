using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service
{
    public class MessageHandlerAttribute : Attribute
    {
        public Type Value { get; private set; }

        public MessageHandlerAttribute(Type type)
        {
            Value = type;
        }

    }
}
