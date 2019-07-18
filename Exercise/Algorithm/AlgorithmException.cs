using System;

namespace Exercise.Algorithm
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
