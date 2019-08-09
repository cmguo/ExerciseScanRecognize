using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
