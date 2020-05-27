using ExtendedHttpClient.Common.Extensions;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Tests
{
    [TestFixture]
    public class GetOperationResultTest
    {
        [Test]
        public async Task GetOperationResultAsync_200()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync("https://httpstat.us/200?sleep=500");
                var operationResult = await result.GetOperationResultAsync<string>(CancellationToken.None);
                Assert.IsTrue(operationResult.IsSuccess);
            }
        }

        [Test]
        public async Task GetOperationResultAsync_404()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync("https://httpstat.us/404?sleep=500");
                var operationResult = await result.GetOperationResultAsync<string>(CancellationToken.None);
                Assert.IsFalse(operationResult.IsSuccess);
                Assert.IsTrue(operationResult.StatusCode == HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetOperationResultAsync_500()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync("https://httpstat.us/500?sleep=500");
                var operationResult = await result.GetOperationResultAsync<string>(CancellationToken.None);
                Assert.IsFalse(operationResult.IsSuccess);
                Assert.IsTrue(operationResult.StatusCode == HttpStatusCode.InternalServerError);
            }
        }
    }
}
