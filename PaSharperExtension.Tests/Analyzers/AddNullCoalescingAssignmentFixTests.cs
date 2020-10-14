using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PaSharperExtension.Analyzers.PossibleNullCoalescingAssignment;

namespace PaSharperExtension.Tests.Analyzers
{
    [TestNetCore30]
    [TestFixture]
    public sealed class AddNullCoalescingAssignmentFixTests : QuickFixTestBase<AddNullCoalescingAssignmentFix>
    {
        protected override string RelativeTestDataPath => @"Analyzers\PossibleNullCoalescingAssignmentQuickFixes";

        [Test]
        public void TestPossibleNullCoalescingAssignment_WithBlock() => DoNamedTest2();

        [Test]
        public void TestPossibleNullCoalescingAssignment_WithoutBlock() => DoNamedTest2();
    }
}
