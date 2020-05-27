using ExtendedHttpClient.Common.Strategies;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Common.Extensions
{
    public static class HttpClientExtension
    {
        public static IResponseStrategy ResponseStrategy { get; set; } = new JsonResponseStrategy();

        public static Task<OperationResult<T>> GetOperationResultAsync<T>(this HttpResponseMessage response, CancellationToken ct)
        {
            return ResponseStrategy.GetOperationResultAsync<T>(response, ct);
        }

        public static Task GetOperationResultAsync<T>(this HttpResponseMessage response, OperationResult<T> operationResult, CancellationToken ct)
        {
            return ResponseStrategy.GetOperationResultAsync<T>(response, operationResult, ct);
        }
    }
}
