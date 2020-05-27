using ExtendedHttpClient.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Common.Strategies
{
    public class JsonResponseStrategy : IResponseStrategy
    {
        private JsonSerializerSettings JsonSerializerSettings { get; set; }


        public JsonResponseStrategy(bool isSnakeCase = false)
            : this(isSnakeCase ? DefaultSnakeCaseJsonSerializerSettings() : DefaultJsonSerializerSettings()) { }

        public JsonResponseStrategy(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings;
        }


        private static JsonSerializerSettings DefaultSnakeCaseJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        private static JsonSerializerSettings DefaultJsonSerializerSettings()
        {
            return new JsonSerializerSettings();
        }


        public virtual async Task<OperationResult<T>> GetOperationResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var result = new OperationResult<T>();

            result.StatusCode = response.StatusCode;
            result.Headers = response.Content?.Headers?.ToArray();

            if (!response.IsSuccessStatusCode)
            {
                var rawResponse = string.Empty;
                if (response.Content != null)
                {
                    rawResponse = await response.Content.ReadAsStringAsync();
                }
                result.Exception = new RequestException(response.RequestMessage.ToString(), rawResponse, response.StatusCode);
                result.RawResponse = rawResponse;
                return result;
            }

            var mediaType = response.Content?.Headers?.ContentType?.MediaType.ToLower();

            if (mediaType != null)
            {
                switch (mediaType)
                {
                    case "text/plain":
                    case "application/json":
                    case "text/html":
                    {
                        try
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            result.RawResponse = content;
                            if (string.IsNullOrEmpty(content))
                            {
                                result.Result = default(T);
                            }
                            else
                            {
                                if (content is T)
                                {
                                    result.Result = (T)(object)content;
                                }
                                else
                                {
                                    result.Result = JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            result.Exception = e;
                        }
                        break;
                    }
                    default:
                    {
                        result.Exception = new InvalidCastException(mediaType);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
