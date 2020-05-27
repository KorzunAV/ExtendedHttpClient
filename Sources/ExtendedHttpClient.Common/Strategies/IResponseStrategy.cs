using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Common.Strategies
{
    public interface IResponseStrategy
    {
        Task<OperationResult<T>> GetOperationResultAsync<T>(HttpResponseMessage response, CancellationToken ct);

        Task GetOperationResultAsync<T>(HttpResponseMessage response, OperationResult<T> operationResult, CancellationToken ct);
    }
}