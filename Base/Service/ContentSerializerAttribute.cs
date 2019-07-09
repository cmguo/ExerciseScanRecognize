using System;

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
