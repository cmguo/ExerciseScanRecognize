using Account.Model;
using Base.Misc;
using Base.Service;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Service
{
    public class AccountHandler : DelegatingHandler
    {
        private string oldUrl = (typeof(IAccount).GetCustomAttributes(typeof(BaseUriAttribute)).First() as BaseUriAttribute).Value;

        private static readonly Logger Log = Logger.GetLogger<AccountHandler>();

        public AccountHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string ticket = AccountModel.Instance.Account.Ticket;
            if (ticket != null)
                request.Headers.TryAddWithoutValidation("ticket", ticket);
            string uri = AccountConfig.Instance.ServiceUri;
            if (uri != null)
            {
                uri = request.RequestUri.ToString().Replace(oldUrl, uri);
                request.RequestUri = new Uri(uri);
            }
            HttpResponseMessage resp = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (resp.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                throw new HttpResponseException(resp.StatusCode, resp.ReasonPhrase);
            return resp;
        }
    }
}
