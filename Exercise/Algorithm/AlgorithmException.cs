using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalBase.Service
{
    class AlgorithmException : Exception
    {

        public int Code { get; private set; }

        public AlgorithmException(int code, string message) : base(message)
        {
            Code = code;
        }

    }
}
