using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace ExtendedHttpClient
{
    public class HttpRetryMessageHandler : DelegatingHandler
    {
        private readonly int _retryCount;

        public HttpRetryMessageHandler(HttpClientHandler handler, int retryCount = 3)
            : base(handler)
        {
            _retryCount = retryCount;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Post || string.Equals(request.Method.Method, "PATCH"))
                return base.SendAsync(request, token);

            var jittered = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(50), _retryCount, null, true);
            return Policy
                 .Handle<HttpRequestException>()
                 .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout)
                 .WaitAndRetryAsync(jittered)
                 .ExecuteAsync(() => base.SendAsync(request, token));
        }
    }
}
