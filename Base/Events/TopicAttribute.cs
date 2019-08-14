using System;

namespace Base.Events
{
    public class TopicAttribute : Attribute
    {
        public string Name { get; private set; }

        public TopicAttribute(string name)
        {
            Name = name;
        }

    }
}
