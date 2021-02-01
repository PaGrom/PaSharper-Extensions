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

            await httpClient.GetAsync("/test", cancellationToken); //http://example.com/test
        }

        async Task BaseAddressInlineInitial(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://example.com/123");

            var method = "/test";

            await httpClient.GetAsync(method, cancellationToken); //http://example.com/123/123/123/test
        }

        async Task UriVariable(CancellationToken cancellationToken)
        {
            var uri = new Uri("http://example.com/123");

            var httpClient = new HttpClient();
            httpClient.BaseAddress = uri;

            var method = "/test";

            await httpClient.GetAsync(method, cancellationToken); //http://example.com/123/123/123/test
        }
    }
}
