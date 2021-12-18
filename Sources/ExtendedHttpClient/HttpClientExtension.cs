using ExtendedHttpClient.Common;
using ExtendedHttpClient.Strategies;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient
{
    public static class HttpClientExtension
    {
        public static IExtendedHttpClientStrategy ClientStrategy { get; set; } = new RestApiClientStrategy();


        public static Task<OperationResult<TOut>> GetAsync<TIn, TOut>(this HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            return ClientStrategy.GetAsync<TIn, TOut>(client, url, data, token);
        }

        public static Task<OperationResult<TOut>> PostAsync<TIn, TOut>(this HttpClient client, string url, TIn data, CancellationToken token)
        {
            return ClientStrategy.PostAsync<TIn, TOut>(client, url, data, token);
        }

        public static Task<OperationResult<TOut>> DeleteAsync<TIn, TOut>(this HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            return ClientStrategy.DeleteAsync<TIn, TOut>(client, url, data, token);
        }

        public static Task<OperationResult<TOut>> PatchAsync<TIn, TOut>(this HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            return ClientStrategy.PatchAsync<TIn, TOut>(client, url, data, token);
        }

        public static Task<OperationResult<TOut>> PutAsync<TIn, TOut>(this HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken))
        {
            return ClientStrategy.PutAsync<TIn, TOut>(client, url, data, token);
        }
    }
}
