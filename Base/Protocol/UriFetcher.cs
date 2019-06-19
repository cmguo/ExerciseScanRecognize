using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Base.Protocol
{
    public class UriFetcher
    {

        public static async Task<Stream> GetStreamAsync(Uri uri)
        {
            if (uri.Scheme == "data")
            {
                DataUtils.DataUrl data = new DataUtils.DataUrl(uri.OriginalString);
                return new MemoryStream(data.Content);
            }
            else if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                HttpClient hc = new HttpClient();
                return await hc.GetStreamAsync(uri);
            }
            else if (uri.Scheme == "file")
            {
                return new FileStream(uri.AbsolutePath, FileMode.Open, FileAccess.Read);
            }
            else
            {
                WebClient wc = new WebClient();
                return await wc.OpenReadTaskAsync(uri);
            }
        }

        public static async Task<byte[]> GetDataAsync(Uri uri)
        {
            if (uri.Scheme == "data")
            {
                DataUtils.DataUrl data = new DataUtils.DataUrl(uri.OriginalString);
                return data.Content;
            }
            else if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                HttpClient hc = new HttpClient();
                return await hc.GetByteArrayAsync(uri);
            }
            else
            {
                WebClient wc = new WebClient();
                return await wc.DownloadDataTaskAsync(uri);
            }
        }

        public static async Task<string> GetStringAsync(Uri uri)
        {
            if (uri.Scheme == "data")
            {
                DataUtils.DataUrl data = new DataUtils.DataUrl(uri.OriginalString);
                return data.ReadAsString();
            }
            else if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                HttpClient hc = new HttpClient();
                return await hc.GetStringAsync(uri);
            }
            else
            {
                WebClient wc = new WebClient();
                return await wc.DownloadStringTaskAsync(uri);
            }
        }
        public static async Task<string> PutStringAsync(Uri uri, Dictionary<String, String> headers, string content)
        {
            if (uri.Scheme == "data")
            {
                DataUtils.DataUrl data = new DataUtils.DataUrl(uri.OriginalString);
                return data.ReadAsString();
            }
            else if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                HttpClient hc = new HttpClient();
                foreach (var h in headers)
                {
                    hc.DefaultRequestHeaders.Remove(h.Key);
                    hc.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                }
                HttpResponseMessage resp = await hc.PutAsync(
                    uri, new StringContent(content, Encoding.UTF8, "application/json"));
                return await resp.Content.ReadAsStringAsync();
            }
            else
            {
                WebClient wc = new WebClient();
                return await wc.DownloadStringTaskAsync(uri);
            }
        }



    }

}
