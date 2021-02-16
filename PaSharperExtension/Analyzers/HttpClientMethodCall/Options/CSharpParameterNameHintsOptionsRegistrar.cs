using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Options;

// ReSharper disable once CheckNamespace
namespace JetBrains.ReSharper.Feature.Services.CSharp.ParameterNameHints
{
    /// <summary>
    /// Register auto refresh inlay hints after oprions change
    /// </summary>
    [SolutionComponent]
    public class CSharpParameterNameHintsOptionsRegistrar
    {
        public CSharpParameterNameHintsOptionsRegistrar(InlayHintsOptionsStore inlayHintsOptionsStore,
            ISettingsStore settingsStore)
        {
            inlayHintsOptionsStore.RegisterSettingsKeyToRehighlightVisibleDocumentOnItsChange(settingsStore.Schema
                .GetKey<HttpClientMethodCallHintOptions>());
        }
    }
}
