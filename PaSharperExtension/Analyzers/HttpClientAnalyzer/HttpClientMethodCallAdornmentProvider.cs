using JetBrains.Application.Settings;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.TextControl.DocumentMarkup;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [SolutionComponent]
    public class HttpClientMethodCallAdornmentProvider : IHighlighterIntraTextAdornmentProvider
    {
        private readonly Lifetime _lifetime;
        private readonly ISettingsStore _settingsStore;

        public HttpClientMethodCallAdornmentProvider(Lifetime lifetime, ISettingsStore settingsStore)
        {
            _lifetime = lifetime;
            _settingsStore = settingsStore;
        }

        public bool IsValid(IHighlighter highlighter)
        {
            return highlighter.UserData is HttpClientMethodCallInfoHint;
        }

        public IIntraTextAdornmentDataModel CreateDataModel(IHighlighter highlighter)
        {
            return highlighter.UserData is HttpClientMethodCallInfoHint hint
                ? new HttpClientMethodCallAdornmentDataModel(_lifetime, _settingsStore, hint.RootVariableDeclarationNode, hint.PathVariableDeclarationNode, hint.UriToCall)
                : null;
        }
    }
}
