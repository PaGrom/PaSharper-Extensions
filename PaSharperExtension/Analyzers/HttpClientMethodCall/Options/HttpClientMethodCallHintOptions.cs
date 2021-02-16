using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.Options
{
    /// <summary>
    /// Inlay Hint options
    /// </summary>
    [SettingsKey(typeof(InlayHintsSettings), "HttpClient Method Call Hint Settings Key")]
    public class HttpClientMethodCallHintOptions
    {
        /// <summary>
        /// Visibility mode
        /// <see cref="InlayHintsMode"/>
        /// </summary>
        [SettingsEntry(InlayHintsMode.Default, "Visibility")]
        public InlayHintsMode VisibilityMode;
    }
}
