using System.Drawing;
using JetBrains.Annotations;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Icons.FeaturesIntellisenseThemedIcons;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.UI.RichText;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.Options
{
    /// <summary>
    /// Options page structure
    /// </summary>
    [OptionsPage(Pid, "HttpClient Method Call Hint", typeof(FeaturesIntellisenseThemedIcons.ParameterInfoPage), ParentId = ToolsPage.PID)]
    public class HttpClientMethodCallHintOptionsPage : BeSimpleOptionsPage
    {
        public const string Pid = "HttpClientMethodCallHintOptions";

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

            AddRichText(new RichText("You can set the default visibility of inlay hints on the ")
                .Append("Environment", new TextStyle(FontStyle.Italic))
                .Append(" | ")
                .Append("Inlay Hints", new TextStyle(FontStyle.Italic))
                .Append(" | ")
                .Append("General", new TextStyle(FontStyle.Italic))
                .Append(" options page"));

            AddRichText(new RichText("General", new TextStyle(FontStyle.Bold)));

            AddComboEnum((HttpClientMethodCallHintOptions key) => key.VisibilityMode, "Visibility:");
        }
    }
}
