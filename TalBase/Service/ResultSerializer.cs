using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TalBase.Service
{
    class ResultSerializer : IContentSerializer
    {
        class Result<T>
        {
            public int status = 0;
            public string message = null;
            public T data;
        }

        private JsonContentSerializer serializer = new JsonContentSerializer();

        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            return serializer.SerializeAsync(item);
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            Result<T> result = await serializer.DeserializeAsync<Result<T>>(content);
            if (result.status != 0)
            {
                throw new ServiceException(result.status, result.message);
            }
            return result.data;
        }

    }
}
