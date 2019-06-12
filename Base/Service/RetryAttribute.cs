using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service
{
    public class RetryAttribute : Attribute
    {

        public int Times { get; private set; }
        public int Interval{ get; private set; }

        public RetryAttribute(int times, int interval)
        {
            Times = times;
            Interval = interval;
        }

    }
}
