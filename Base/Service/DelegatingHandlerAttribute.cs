using System;

namespace Base.Service
{

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class DelegatingHandlerAttribute : Attribute
    {
        public Type Value { get; private set; }

        public DelegatingHandlerAttribute(Type type)
        {
            Value = type;
        }

    }
}
