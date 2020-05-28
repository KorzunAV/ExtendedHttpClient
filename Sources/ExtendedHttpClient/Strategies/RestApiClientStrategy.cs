using ExtendedHttpClient.Common;
using ExtendedHttpClient.Common.Attributes;
using ExtendedHttpClient.Common.Strategies;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Strategies
{
    public class RestApiClientStrategy : IExtendedHttpClientStrategy
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IResponseStrategy _responseStrategy;
        private readonly Dictionary<Type, PropertyInfo[]> _cash;

        public int PollyRetryCount { get; set; } = 5;


        public RestApiClientStrategy(bool isSnakeCase = false)
            : this(isSnakeCase ? JsonSerializerSettingsHelper.SnakeCaseJsonSerializerSettings : JsonSerializerSettingsHelper.JsonSerializerSettings) { }

        public RestApiClientStrategy(JsonSerializerSettings jsonSerializerSettings)
            : this(jsonSerializerSettings, new JsonResponseStrategy(jsonSerializerSettings)) { }

        public RestApiClientStrategy(JsonSerializerSettings jsonSerializerSettings, IResponseStrategy responseStrategy)
        {
            _cash = new Dictionary<Type, PropertyInfo[]>();
            _jsonSerializerSettings = jsonSerializerSettings;
            _responseStrategy = responseStrategy;
        }


        public Task<OperationResult<TOut>> GetAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = GetUri(url, data)
            };

            return TrySendAsync<TIn, TOut>(client, request, data, PollyRetryCount, token);
        }

        public Task<OperationResult<TOut>> PostAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = GetUri(url, data),
                Content = GetContent(data),
            };

            return TrySendAsync<TIn, TOut>(client, request, data, 0, token);
        }

        public Task<OperationResult<TOut>> DeleteAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = GetUri(url, data),
                Content = GetContent(data)
            };

            return TrySendAsync<TIn, TOut>(client, request, data, PollyRetryCount, token);
        }

        public Task<OperationResult<TOut>> PatchAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = GetUri(url, data),
                Content = GetContent(data)
            };

            return TrySendAsync<TIn, TOut>(client, request, data, 0, token);
        }

        public Task<OperationResult<TOut>> PutAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = GetUri(url, data),
                Content = GetContent(data)
            };

            return TrySendAsync<TIn, TOut>(client, request, data, PollyRetryCount, token);
        }


        public Uri GetUri<TIn>(string url, TIn data)
        {
            var q = GetQuery(data);

            return string.IsNullOrEmpty(q)
                ? new Uri(url)
                : new Uri($"{url}?{q}");
        }

        public string GetQuery<TIn>(TIn container)
        {
            if (container == null)
                return string.Empty;

            var args = new List<string>();
            var containerProperties = GetProperties<TIn>();
            foreach (var prop in containerProperties)
            {
                var fromQueryAttribute = prop.GetCustomAttribute<QueryAttribute>();
                if (fromQueryAttribute == null)
                    continue;

                var qValue = prop.GetValue(container);
                if (qValue == null)
                    break;

                var properties = GetProperties(prop.PropertyType);
                
                foreach (var property in properties)
                {
                    var pName = property.GetCustomAttribute<JsonPropertyAttribute>();
                    var obj = property.GetValue(qValue);
                    if (obj != null)
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            var value = property.PropertyType
                                .GetTypeInfo()
                                .DeclaredMembers
                                .SingleOrDefault(x => x.Name == obj.ToString())
                                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                                ?.Value;
                            args.Add($"{pName.PropertyName}={value}");
                        }
                        else
                        {
                            args.Add($"{pName.PropertyName}={obj}");
                        }
                    }
                }
                break;
            }
            if (args.Any())
                return string.Join("&", args);

            return string.Empty;
        }


        private void AddHeaders<T>(HttpRequestMessage request, T data)
        {
            var containerProperties = GetProperties<T>();
            foreach (var prop in containerProperties)
            {
                var headerAttribute = prop.GetCustomAttribute<HeaderAttribute>();
                if (headerAttribute == null)
                    continue;

                if (headerAttribute.IsAuthorization)
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        var typed = prop.GetValue(data) as string;
                        request.Headers.Authorization = new AuthenticationHeaderValue(headerAttribute.HeaderKey, typed);
                    }
                    else
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(headerAttribute.HeaderKey);
                    }
                }
                else
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        var typed = prop.GetValue(data) as string;
                        request.Headers.Add(headerAttribute.HeaderKey, typed);
                    }
                    else if (prop.PropertyType.IsArray && prop.PropertyType.GetElementType() == typeof(string))
                    {
                        var typed = prop.GetValue(data) as IEnumerable<string>;
                        request.Headers.Add(headerAttribute.HeaderKey, typed);
                    }
                }
            }
        }

        private async Task<OperationResult<TOut>> TrySendAsync<TIn, TOut>(HttpClient client, HttpRequestMessage request, TIn data, int retryCount, CancellationToken token)
        {
            var result = new OperationResult<TOut>();
            try
            {
                AddHeaders(request, data);
                result.RawRequest = $"{request}";
                if (request.Content is StringContent sc)
                {
                    var t = await sc.ReadAsStringAsync();
                    result.RawRequest += $"{Environment.NewLine}Body: {t}";
                }

                HttpResponseMessage response;
                if (retryCount > 0)
                {
                    response = await Policy
                        .Handle<TaskCanceledException>()
                        .WaitAndRetryAsync
                        (
                            retryCount: retryCount,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await client.SendAsync(request, token).ConfigureAwait(false));
                }
                else
                {
                    response = await client.SendAsync(request, token).ConfigureAwait(false);
                }


                await _responseStrategy.GetOperationResultAsync<TOut>(response, result, token);

            }
            catch (TaskCanceledException ex)
            {
                if (token.IsCancellationRequested)
                    throw;

                result.Exception = ex;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        private PropertyInfo[] GetProperties<T>()
        {
            var type = typeof(T);
            return GetProperties(type);
        }

        private PropertyInfo[] GetProperties(Type type)
        {
            if (_cash.ContainsKey(type))
            {
                return _cash[type];
            }
            var props = type.GetRuntimeProperties().ToArray();
            _cash.Add(type, props);
            return props;
        }

        private HttpContent GetContent<TIn>(TIn data)
        {
            if (data == null)
                return null;

            var props = GetProperties<TIn>();
            foreach (var prop in props)
            {
                var fromBodyAttribute = prop.GetCustomAttribute<BodyAttribute>();
                if (fromBodyAttribute == null)
                    continue;
                switch (fromBodyAttribute.Type)
                {
                    case BodyMimeType.ApplicationJson:
                    {
                        var obj = prop.GetValue(data);
                        var json = JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
                        return new StringContent(json, Encoding.UTF8, "application/json");
                    }
                }
            }
            return null;
        }
    }
}