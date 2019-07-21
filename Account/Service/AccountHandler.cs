using Account.Model;
using Base.Service;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Service
{
    public class AccountHandler : HttpClientHandler
    {
        private string oldUrl = (typeof(IAccount).GetCustomAttributes(typeof(BaseUriAttribute)).First() as BaseUriAttribute).Value;

        public AccountHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string ticket = AccountModel.Instance.Account.Ticket;
            if (ticket != null)
                request.Headers.TryAddWithoutValidation("ticket", ticket);
            string uri = AccountModel.Instance.ServiceUri;
            if (uri != null)
            {
                uri = request.RequestUri.ToString().Replace(oldUrl, uri);
                request.RequestUri = new Uri(uri);
            }
            Debug.WriteLine(request.RequestUri);
            HttpResponseMessage resp = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (resp.StatusCode.CompareTo(HttpStatusCode.Ambiguous) >= 0)
                throw new HttpResponseException(resp.StatusCode, resp.ReasonPhrase);
            return resp;
        }
    }
}
