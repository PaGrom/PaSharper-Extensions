using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.Application.Zones;

namespace PaSharperExtension.Tests
{
    [ZoneDefinition]
    public interface IPaSharperExtensionTestZone : ITestsEnvZone, IRequire<IPaSharperExtensionZone>, IRequire<PsiFeatureTestZone> { }

    [ZoneMarker]
    public sealed class ZoneMarker : IRequire<IPaSharperExtensionZone> { }
}