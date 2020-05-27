using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Common.Strategies
{
    public interface IResponseStrategy
    {
        Task<OperationResult<T>> GetOperationResultAsync<T>(HttpResponseMessage response, CancellationToken ct);
    }
}