using System;

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
