using ExtendedHttpClient.Common.Strategies;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Common.Extensions
{
    public static class HttpClientExtension
    {
        public static IResponseStrategy ResponseStrategy { get; set; } = new JsonResponseStrategy();

        public static Task<OperationResult<T>> GetOperationResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            return ResponseStrategy.GetOperationResultAsync<T>(response, ct);
        }
    }
}
