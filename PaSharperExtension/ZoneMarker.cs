using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.Psi.CSharp;
using PaSharperExtension;

namespace PaSharperExtension
{
    [ZoneDefinition]
    [ZoneDefinitionConfigurableFeature(
        "PaSharper Extensions for ReSharper",
        "Just for fun extensions",
        false)]
    public interface IPaSharperExtensionZone : IZone, IRequire<ILanguageCSharpZone> { }

    [ZoneMarker]
    public sealed class ZoneMarker : IRequire<IPaSharperExtensionZone> { }
}

namespace ExtensionActivator
{
    [ZoneActivator]
    [ZoneMarker]
    public sealed class PaSharperExtensionActivator : IActivate<IPaSharperExtensionZone>
    {
        public bool ActivatorEnabled() => true;
    }
}
