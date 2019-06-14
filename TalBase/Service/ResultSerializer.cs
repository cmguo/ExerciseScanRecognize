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
            public int Status { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }

            internal void Check()
            {
                if (Status != 0)
                    throw new ServiceException(Status, Message);
            }
        }

        class Result2<T>
        {
            public bool Success { get; set; }
            public int ErrorCode { get; set; }
            [JsonProperty(PropertyName = "result")]
            public T Data { get; set; }

            internal void Check()
            {
                if (!Success)
                    throw new ServiceException(ErrorCode, "");
            }
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
