using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer.Options
{
    [SettingsKey(typeof(EnvironmentSettings), "HttpClient Method Call Hint Settings Key")]
    public class HttpClientMethodCallHintSettingsKey
    {
        [SettingsEntry(InlayHintsMode.Default, "Visibility")]
        public InlayHintsMode VisibilityMode;
    }
}
