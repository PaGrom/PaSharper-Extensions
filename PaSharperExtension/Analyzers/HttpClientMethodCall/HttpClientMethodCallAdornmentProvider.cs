using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.TextControl.DocumentMarkup;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall
{
    [SolutionComponent]
    public class HttpClientMethodCallAdornmentProvider : IHighlighterIntraTextAdornmentProvider
    {
        private readonly ISettingsStore _settingsStore;

        public HttpClientMethodCallAdornmentProvider(ISettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
        }

        public bool IsValid(IHighlighter highlighter)
        {
            return highlighter.UserData is HttpClientMethodCallInfoHint;
        }

        public IIntraTextAdornmentDataModel CreateDataModel(IHighlighter highlighter)
        {
            return highlighter.UserData is HttpClientMethodCallInfoHint hint
                ? new HttpClientMethodCallAdornmentDataModel(_settingsStore, hint)
                : null;
        }
    }
}
