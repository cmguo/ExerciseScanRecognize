using System;

namespace Base.Service
{
    public class BaseUriAttribute : Attribute
    {

        public string Value { get; private set; }

        public BaseUriAttribute(string path)
        {
            Value = path;
        }

    }
}
