using System;

namespace Base.Service
{
    public class DelegatingHandlerAttribute : Attribute
    {
        public Type Value { get; private set; }

        public DelegatingHandlerAttribute(Type type)
        {
            Value = type;
        }

    }
}
