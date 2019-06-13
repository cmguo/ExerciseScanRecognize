using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace TalBase.Service
{
    public class ResultSerializer : IContentSerializer
    {
        class Result<T>
        {
            public int status = 0;
            public string message = null;
            public T data;

            internal void Check()
            {
                if (status != 0)
                    throw new ServiceException(status, message);
            }
            internal T Data { get { return data; } }
        }

        class Result2<T>
        {
            public bool success;
            public int errorCode;
            public T result;

            internal void Check()
            {
                if (!success)
                    throw new ServiceException(errorCode, "");
            }

            internal T Data { get { return result; } }
        }

        private JsonContentSerializer serializer = new JsonContentSerializer(new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            return serializer.SerializeAsync(item);
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            var result = await serializer.DeserializeAsync<Result2<T>>(content);
            result.Check();
            return result.Data;
        }

    }
}
