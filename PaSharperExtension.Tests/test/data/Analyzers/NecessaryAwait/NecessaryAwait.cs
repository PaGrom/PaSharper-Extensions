using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PaSharperExtension.Tests.test.data.Analyzers.NecessaryAwait
{
    public class AwaitForMethods
    {
        Task Method()
        {
            using (var httpClient = new HttpClient())
            {
                return httpClient.GetAsync("url");
            }
        }
    }
}
