using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Model
{
    class ModelException : Exception
    {

        private int _status;

        public ModelException(int status, string message) : base(message)
        {
            _status = status;
        }

        public int GetStatus()
        {
            return _status;
        }
    }
}
