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
            string rootUrl, testMethod;

            rootUrl = "http://example.com/";

            rootUrl += "1/2/3";

            rootUrl += "/4";

            testMethod = "test";

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(rootUrl)
            };

            await httpClient.GetAsync("/test", cancellationToken);
            await httpClient.GetAsync(testMethod, cancellationToken);
        }

        async Task BaseAddressInlineInitial(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://example.com/123/");

            var method = "test";

            await httpClient.GetAsync(method, cancellationToken);
        }

        async Task UriVariable(CancellationToken cancellationToken)
        {
            var uri = new Uri("http://example.com/123");

            var httpClient = new HttpClient();
            httpClient.BaseAddress = uri;

            var method = "test";

            await httpClient.GetAsync(method, cancellationToken);
        }

        async Task UriVariable(bool a, CancellationToken cancellationToken)
        {
            var uri = new Uri("http://example.com/123");

            var httpClient = new HttpClient();
            httpClient.BaseAddress = uri;

            var method = "test";

            if (a)
            {
                method = "1/2/3";
            }

            await httpClient.GetAsync(method, cancellationToken);
        }
    }

    public class HttpClientMethodCall
    {
        public const string ConstRootUrl = "http://example.com/123/";

        async Task BaseAddressInlineInitial(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ConstRootUrl);

            var method = "test";

            await httpClient.GetAsync(method, cancellationToken);
        }
    }
}
