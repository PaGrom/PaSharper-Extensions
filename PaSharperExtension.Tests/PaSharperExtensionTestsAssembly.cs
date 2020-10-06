using System.Threading;
using JetBrains.TestFramework;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace PaSharperExtension.Tests
{
    [SetUpFixture]
    public class PaSharperExtensionTestsAssembly : ExtensionTestEnvironmentAssembly<IPaSharperExtensionTestZone> { }
}
