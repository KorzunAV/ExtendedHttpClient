using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ExtendedHttpClient.Tests
{
    [TestFixture]
    public class HttpRetryMessageHandlerTest
    {
        //{ "hello": "world" }
        private const string TestUrl = "https://httpstat.us/500?sleep=5000";

        [Test]
        public async Task RepeatTest_Success()
        {
            using (var client = new HttpClient(new HttpRetryMessageHandler(new HttpClientHandler())))
            {
                var result = await client.GetAsync(TestUrl );
                Assert.IsTrue(result.IsSuccessStatusCode);
                Console.WriteLine(await result.Content.ReadAsStringAsync());
            }
        }
    }
}
