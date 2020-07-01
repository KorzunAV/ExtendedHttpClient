using ExtendedHttpClient.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Common.Strategies
{
    public class JsonResponseStrategy : IResponseStrategy
    {
        public HashSet<string> SupportedMimiTypes { get; private set; }

        private JsonSerializerSettings JsonSerializerSettings { get; set; }


        public JsonResponseStrategy(bool isSnakeCase = false)
            : this(isSnakeCase ? JsonSerializerSettingsHelper.SnakeCaseJsonSerializerSettings : JsonSerializerSettingsHelper.JsonSerializerSettings) { }

        public JsonResponseStrategy(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings;
            SupportedMimiTypes = new HashSet<string>
            {
                "text/plain",
                "text/html",
                "text/javascript",
                "application/json",
            };
        }


        public virtual async Task<OperationResult<T>> GetOperationResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var result = new OperationResult<T>();
            result.RawRequest = response.RequestMessage.ToString();
            await GetOperationResultAsync(response, result, ct);
            return result;
        }

        public async Task GetOperationResultAsync<T>(HttpResponseMessage response, OperationResult<T> operationResult, CancellationToken ct)
        {
            operationResult.StatusCode = response.StatusCode;
            operationResult.Headers = response.Content?.Headers?.ToArray();

            if (!response.IsSuccessStatusCode)
            {
                var rawResponse = string.Empty;
                if (response.Content != null)
                {
                    rawResponse = await response.Content.ReadAsStringAsync();
                }
                operationResult.Exception = new RequestException(response.RequestMessage.ToString(), rawResponse, response.StatusCode);
                operationResult.RawResponse = rawResponse;
                return;
            }

            var mediaType = response.Content?.Headers?.ContentType?.MediaType.ToLower();

            if (mediaType != null)
            {
                if (!SupportedMimiTypes.Contains(mediaType))
                    operationResult.Exception = new InvalidCastException(mediaType);
                
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    operationResult.RawResponse = content;
                    if (string.IsNullOrEmpty(content))
                    {
                        operationResult.Result = default(T);
                    }
                    else
                    {
                        if (content is T)
                        {
                            operationResult.Result = (T)(object)content;
                        }
                        else
                        {
                            operationResult.Result = JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);
                        }
                    }
                }
                catch (Exception e)
                {
                    operationResult.Exception = e;
                }
            }
        }
    }
}
