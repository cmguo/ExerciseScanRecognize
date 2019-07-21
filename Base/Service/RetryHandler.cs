using Base.Misc;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Service
{
    public class RetryHandler : DelegatingHandler
    {
        private static readonly Logger Log = Logger.GetLogger<RetryHandler>();

        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private RetryAttribute Retry;

        public RetryHandler(HttpMessageHandler innerHandler, RetryAttribute retry)
            : base(innerHandler)
        {
            Retry = retry;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            int t = Retry.Times;
            while (true)
            {
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (Exception e)
                {
                    Log.w(e);
                    if (t == 0 || !Recoverable(e))
                        throw;
                    --t;
                    await Task.Delay(Retry.Interval);
                }
            }
        }

        private bool Recoverable(Exception e)
        {
            return e is SocketException
                || e is HttpRequestException;
        }
    }
}
