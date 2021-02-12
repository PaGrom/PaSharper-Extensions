using JetBrains.Annotations;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Resources;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer.Options
{
    [OptionsPage(Pid, "HttpClient Method Call Hint", typeof(FeaturesEnvironmentOptionsThemedIcons.CodeInspections), ParentId = ToolsPage.PID)]
    public class HttpClientMethodCallHintOptionsPage : BeSimpleOptionsPage
    {
        public const string Pid = "HttpClientMethodCallHintOptions";

        public HttpClientMethodCallHintOptionsPage(Lifetime lifetime,
            OptionsPageContext optionsPageContext,
            [NotNull] OptionsSettingsSmartContext optionsSettingsSmartContext)
            : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
        {
            IProperty<InlayHintsMode> visibilityMode = new Property<InlayHintsMode>(lifetime, "HttpClientMethodCallHintOptionsPage::VisibilityMode");
            visibilityMode.SetValue(
                optionsSettingsSmartContext.StoreOptionsTransactionContext.GetValue(
                    (HttpClientMethodCallHintSettingsKey key) => key.VisibilityMode));

            visibilityMode.Change.Advise(lifetime, a =>
            {
                if (!a.HasNew) return;
                optionsSettingsSmartContext.StoreOptionsTransactionContext.SetValue(
                    (HttpClientMethodCallHintSettingsKey key) => key.VisibilityMode, a.New);
            });

            AddComboEnum((HttpClientMethodCallHintSettingsKey key) => key.VisibilityMode, "Visibility:");
        }
    }
}
