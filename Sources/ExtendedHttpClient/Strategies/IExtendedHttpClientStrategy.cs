using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExtendedHttpClient.Common;

namespace ExtendedHttpClient.Strategies
{
    public interface IExtendedHttpClientStrategy
    {
        Task<OperationResult<TOut>> GetAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken));
        Task<OperationResult<TOut>> PostAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token);
        Task<OperationResult<TOut>> DeleteAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken));
        Task<OperationResult<TOut>> PatchAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token = default(CancellationToken));
        Task<OperationResult<TOut>> PutAsync<TIn, TOut>(HttpClient client, string url, TIn data, CancellationToken token);
    }
}