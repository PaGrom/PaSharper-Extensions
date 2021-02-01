using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PaSharperExtension.Analyzers.HttpClientAnalyzer;

namespace PaSharperExtension.Tests.Analyzers
{
    [TestNetCore30]
    [TestFixture]
    public sealed class HttpClientMethodCallAnalyzerTests : CSharpHighlightingTestBase
    {
        protected override string RelativeTestDataPath => @"Analyzers\HttpClientMethodCall";

        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
            => highlighting is HttpClientMethodCallSuggestion;

        [Test]
        public void TestHttpClientMethodCall() => DoNamedTest2();
    }
}
