using Base.Config;
using Base.Misc;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Service
{

    [DebugOnly]
    public class LoggingHandler : DelegatingHandler
    {

        private static readonly Logger Log = Logger.GetLogger<LoggingHandler>();

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Log.i("Request: " + request.ToString());
            if (request.Content != null)
            {
                Log.d(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Log.d("Response: " + response.ToString());
            if (response.Content != null)
            {
                Log.d(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }

}
