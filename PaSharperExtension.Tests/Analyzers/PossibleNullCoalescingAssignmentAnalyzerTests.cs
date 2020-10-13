using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PaSharperExtension.Analyzers.PossibleNullCoalescingAssignment;

namespace PaSharperExtension.Tests.Analyzers
{
    [TestNetCore30]
    [TestFixture]
    public sealed class PossibleNullCoalescingAssignmentAnalyzerTests : CSharpHighlightingTestBase
    {
        protected override string RelativeTestDataPath => @"Analyzers\PossibleNullCoalescingAssignment";

        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
            => highlighting is NullCoalescingAssignmentSuggestion;

        [Test]
        public void TestPossibleNullCoalescingAssignment() => DoNamedTest2();
    }
}