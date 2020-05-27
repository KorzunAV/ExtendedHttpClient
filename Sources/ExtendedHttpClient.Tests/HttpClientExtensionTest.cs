using ExtendedHttpClient.Common.Attributes;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendedHttpClient.Tests
{
    [TestFixture]
    public class HttpClientExtensionTest
    {
        public class Request
        {
            [Query]
            public QueryArgs Query { get; set; }

            public class QueryArgs
            {
                [JsonProperty("sleep")]
                public int Sleep { get; set; }
            }
        }

        [Test]
        public async Task GetOperationResultAsync_200()
        {
            var args = new Request
            {
                Query = new Request.QueryArgs
                {
                    Sleep = 500
                }
            };
            using (var client = new HttpClient())
            {
                var operationResult = await client.GetAsync<Request, string>("https://httpstat.us/200", args, CancellationToken.None);
                Assert.IsTrue(operationResult.IsSuccess);
            }
        }

        [Test]
        public async Task GetOperationResultAsync_404()
        {
            var args = new Request
            {
                Query = new Request.QueryArgs
                {
                    Sleep = 500
                }
            };
            using (var client = new HttpClient())
            {
                var operationResult = await client.GetAsync<Request, string>("https://httpstat.us/404", args, CancellationToken.None);
                Assert.IsFalse(operationResult.IsSuccess);
                Assert.IsTrue(operationResult.StatusCode == HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetOperationResultAsync_500()
        {
            var args = new Request
            {
                Query = new Request.QueryArgs
                {
                    Sleep = 500
                }
            };
            using (var client = new HttpClient())
            {
                var operationResult = await client.GetAsync<Request, string>("https://httpstat.us/500", args, CancellationToken.None);
                Assert.IsFalse(operationResult.IsSuccess);
                Assert.IsTrue(operationResult.StatusCode == HttpStatusCode.InternalServerError);
            }
        }
    }
}
