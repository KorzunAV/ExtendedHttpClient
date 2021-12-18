using ExtendedHttpClient.Common;
using ExtendedHttpClient.Common.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient
{
    [Obsolete("Try to use HttpClientExtension")]
    public class ExtendedHttpClient : HttpClient
    {
        public int PollyRetryCount { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
        public bool IsOpenApiFormat { get; set; }

        public ExtendedHttpClient()
        {
            ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            PollyRetryCount = 5;
            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            MaxResponseContentBufferSize = 2560000;
            Timeout = TimeSpan.FromSeconds(15);
            DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            IsOpenApiFormat = true;
        }


        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Options"/> request as an synchronous operation. Auto-repeat request on error.
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> OptionsAsync<TOut>(string url, CancellationToken token = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Options,
                RequestUri = new Uri(url)
            };

            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }

        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Head"/> request as an synchronous operation. Auto-repeat request on error.
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> HeadAsync<TOut>(string url, CancellationToken token = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Head,
                RequestUri = new Uri(url)
            };

            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }

        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Get"/> request as an synchronous operation. Auto-repeat request on error.  
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> GetAsync<TOut>(string url, CancellationToken token = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }

        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Get"/> request as an synchronous operation. Auto-repeat request on error.  
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> GetAsync<TIn, TOut>(string url, TIn args, CancellationToken token = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = CreateUriFromModel(url, args)
            };
            AddHeader(request, args);

            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }

        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Post"/> request as an synchronous operation.
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> PostAsync<TIn, TOut>(string url, TIn args, CancellationToken token)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = GetContent(args),
            };

            return TrySendAsync<TOut>(request, 0, token);
        }

        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Delete"/> request as an synchronous operation. Auto-repeat request on error.
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> DeleteAsync<TIn, TOut>(string url, TIn args, CancellationToken token = default)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url),
                Content = GetContent(args)
            };

            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }

        /// <summary>
        /// Send an HTTP "PATCH" request as an synchronous operation.
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> PatchAsync<TIn, TOut>(string url, TIn args, CancellationToken token = default)
        {
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url),
                Content = GetContent(args)
            };

            return TrySendAsync<TOut>(request, 0, token);
        }

        /// <summary>
        /// Send an HTTP <see cref="HttpMethod.Put"/> request as an synchronous operation. Auto-repeat request on error. 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> PutAsync<TIn, TOut>(string url, TIn args, CancellationToken token)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(url),
                Content = GetContent(args)
            };

            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }

        /// <summary>
        /// Send an HTTP request as an synchronous operation. Auto-repeat request on error.  
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="request"></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        public Task<OperationResult<TOut>> SendAsync<TOut>(HttpRequestMessage request, CancellationToken token)
        {
            return TrySendAsync<TOut>(request, PollyRetryCount, token);
        }


        /// <summary>
        /// Send an HTTP request as an synchronous operation. Auto-repeat idempotent request on error.  
        /// </summary>
        /// <typeparam name="TOut">Custom response type</typeparam>
        /// <param name="request"><see cref="T:System.OperationCanceledException" /></param>
        /// <param name="retryCount">The number of possible repeated requests. Used for <see cref="o:Polly.AsyncRetrySyntax.WaitAndRetryAsync"/></param>
        /// <param name="token">Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</param>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <returns></returns>
        protected virtual async Task<OperationResult<TOut>> TrySendAsync<TOut>(HttpRequestMessage request, int retryCount, CancellationToken token)
        {
            try
            {
                HttpResponseMessage response;
                if (retryCount < 0)
                {
                    response = await Policy
                        .Handle<TaskCanceledException>()
                        .WaitAndRetryAsync
                        (
                            retryCount: retryCount,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await SendAsync(request, token)
                            .ConfigureAwait(false));
                }
                else
                {
                    response = await SendAsync(request, token)
                        .ConfigureAwait(false);
                }

                return await CreateResultAsync<TOut>(response, token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                if (token.IsCancellationRequested)
                    throw;

                return new OperationResult<TOut>(ex);
            }
            catch (Exception ex)
            {
                return new OperationResult<TOut>(ex);
            }
        }

        private Uri CreateUriFromModel<TIn>(string url, TIn args)
        {
            var urlBuilder = new UriBuilder(url);

            if (args == null)
                return urlBuilder.Uri;

            var props = typeof(TIn).GetRuntimeProperties().ToArray();

            var query = new NameValueCollection();

            foreach (var prop in props)
            {
                if (IsOpenApiFormat)
                {
                    var fromQueryAttribute = prop.GetCustomAttribute<QueryAttribute>();
                    if (fromQueryAttribute == null)
                        continue;
                }

                var pName = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var obj = prop.GetValue(args);

                if (obj == null)
                    continue;
                query[pName.PropertyName] = obj.ToString();
            }

            urlBuilder.Query = query.ToString();
            return urlBuilder.Uri;
        }



        private HttpContent GetContent<TIn>(TIn data)
        {
            if (data == null)
                return null;

            if (IsOpenApiFormat)
            {
                var props = typeof(TIn).GetRuntimeProperties().ToArray();

                foreach (var prop in props)
                {
                    var fromBodyAttribute = prop.GetCustomAttribute<BodyAttribute>();
                    if (fromBodyAttribute == null)
                        continue;

                    var obj = prop.GetValue(data);
                    var json = JsonConvert.SerializeObject(obj, JsonSerializerSettings);
                    return new StringContent(json, Encoding.UTF8, "application/json");
                }
            }
            else
            {
                var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
                return new StringContent(json, Encoding.UTF8, "application/json");
            }

            return null;
        }


        protected virtual async Task<OperationResult<T>> CreateResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var result = new OperationResult<T>();
            result.RawRequest = response.RequestMessage.ToString();
            result.StatusCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                var rawResponse = await response.Content.ReadAsStringAsync();
                result.Exception = new HttpRequestException(response.RequestMessage.ToString());
                result.RawResponse = rawResponse;
                return result;
            }

            if (response.Content == null)
                return result;

            var mediaType = response.Content.Headers?.ContentType?.MediaType.ToLower();

            if (mediaType != null)
            {
                switch (mediaType)
                {
                    case "text/plain":
                    case "application/json":
                    case "text/html":
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        result.RawResponse = content;
                        if (string.IsNullOrEmpty(content))
                            result.Result = default;
                        else
                            result.Result = JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);
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

        protected virtual void AddHeader<TIn>(HttpRequestMessage request, TIn args)
        {
            if (args == null)
                return;

            var props = typeof(TIn).GetRuntimeProperties().ToArray();

            foreach (var prop in props)
            {
                var headerAttribute = prop.GetCustomAttribute<HeaderAttribute>();
                if (headerAttribute == null)
                    continue;

                var jsonProperty = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var obj = prop.GetValue(args);
                request.Headers.Add(jsonProperty.PropertyName, obj.ToString());
            }
        }
    }
}
