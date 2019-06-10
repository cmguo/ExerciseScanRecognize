using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalBase.Service
{
    class ServiceException : Exception
    {

        private int _status;

        public ServiceException(int status, string message) : base(message)
        {
            _status = status;
        }

        public int GetStatus()
        {
            return _status;
        }
    }
}
