using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using PaSharperExtension.Analyzers.CharInsteadOfInt;

namespace PaSharperExtension.Tests.Analyzers
{
    [TestNetCore30]
    [TestFixture]
    public sealed class CharInsteadOfIntFixTests : QuickFixTestBase<CharInsteadOfIntFix>
    {
        protected override string RelativeTestDataPath => @"Analyzers\CharInsteadOfIntQuickFix";

        [Test]
        public void TestCharInsteadOfInt() => DoNamedTest2();
    }
}
