using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PaSharperExtension.Analyzers.HttpClientAnalyzer;

namespace PaSharperExtension.Tests.Analyzers
{
    [TestNetCore30]
    [TestFixture]
    public sealed class HttpClientMethodCallFixTests : QuickFixTestBase<HttpClientMethodCallFix>
    {
        protected override string RelativeTestDataPath => @"Analyzers\HttpClientMethodCallFixTests";

        [Test]
        public void TestHttpClientMethodCall() => DoNamedTest2();
    }
}
