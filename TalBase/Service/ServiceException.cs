using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalBase.Service
{
    public class ServiceException : Exception
    {

        public int Status { get; private set; }

        public ServiceException(int status, string message) : base(message)
        {
            Status = status;
        }

    }
}
