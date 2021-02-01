using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PaSharperExtension.Tests.test.data.Analyzers.HttpClientMethodCall
{
    public class HttpClientMethodCall
    {
        async Task BaseAdreesInsideInit(CancellationToken cancellationToken)
        {
            var rootUrl = "http://example.com/";

            rootUrl += "123/123/123";

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(rootUrl)
            };

            await {caret}httpClient.GetAsync("/test", cancellationToken); //http://example.com/test
        }
    }
}
