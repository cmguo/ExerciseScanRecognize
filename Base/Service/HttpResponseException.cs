using System;
using System.Net;

namespace Base.Service
{
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpResponseException(HttpStatusCode status, string message)
            : base((message == null || message.Length == 0) ? "Http " + status.ToString() : message)
        {
            StatusCode = status;
        }
    }
}
