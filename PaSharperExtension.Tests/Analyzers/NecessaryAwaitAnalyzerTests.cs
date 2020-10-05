using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PaSharperExtension.Analyzers.NecessaryAwait;

namespace PaSharperExtension.Tests.Analyzers
{
    [TestNetCore30]
    [TestFixture]
    public sealed class NecessaryAwaitAnalyzerTests : CSharpHighlightingTestBase
    {
        protected override string RelativeTestDataPath => @"Analyzers\NecessaryAwait";

        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
            => highlighting is NecessaryAwaitSuggestion;

        [Test]
        public void TestNecessaryAwait() => DoNamedTest2();
    }
}