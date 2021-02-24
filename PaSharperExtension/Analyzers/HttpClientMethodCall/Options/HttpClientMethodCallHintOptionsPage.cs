using JetBrains.Annotations;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Icons.FeaturesIntellisenseThemedIcons;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.CSharp.ParameterNameHints;
using JetBrains.ReSharper.Feature.Services.InlayHints;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.Options
{
    /// <summary>
    /// Options page structure
    /// </summary>
    [OptionsPage(Pid, "[PaSharper] HttpClient Method Call Hint", typeof(FeaturesIntellisenseThemedIcons.ParameterInfoPage), ParentId = CSharpOtherInlayHintsOptionsPage.PID)]
    public class HttpClientMethodCallHintOptionsPage : InlayHintsOptionPageBase
    {
        public const string Pid = "PaSharper.HttpClientMethodCallHintOptions";

        public HttpClientMethodCallHintOptionsPage(Lifetime lifetime,
            OptionsPageContext optionsPageContext,
            [NotNull] OptionsSettingsSmartContext optionsSettingsSmartContext)
            : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
        {
            IProperty<InlayHintsMode> visibilityMode = new Property<InlayHintsMode>(lifetime,
                $"{nameof(HttpClientMethodCallHintOptionsPage)}::{nameof(HttpClientMethodCallHintOptions.VisibilityMode)}");

            visibilityMode.SetValue(
                optionsSettingsSmartContext.StoreOptionsTransactionContext.GetValue(
                    (HttpClientMethodCallHintOptions key) => key.VisibilityMode));

            visibilityMode.Change.Advise(lifetime, a =>
            {
                if (!a.HasNew) return;
                optionsSettingsSmartContext.StoreOptionsTransactionContext.SetValue(
                    (HttpClientMethodCallHintOptions key) => key.VisibilityMode, a.New);
            });

            AddVisibilityHelpText();

            AddVisibilityOption((HttpClientMethodCallHintOptions key) => key.VisibilityMode);
        }
    }
}
