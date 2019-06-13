using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service
{
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpResponseException(HttpStatusCode status, string message)
            : base(message == null ? "Http " + status.ToString() : message)
        {
            StatusCode = status;
        }
    }
}
