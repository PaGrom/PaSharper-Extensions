using JetBrains.ProjectModel;
using JetBrains.TextControl.DocumentMarkup;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [SolutionComponent]
    public class HttpClientMethodCallAdornmentProvider : IHighlighterIntraTextAdornmentProvider
    {
        public bool IsValid(IHighlighter highlighter)
        {
            return highlighter.UserData is HttpClientMethodCallInfoHint;
        }

        public IIntraTextAdornmentDataModel CreateDataModel(IHighlighter highlighter)
        {
            return highlighter.UserData is HttpClientMethodCallInfoHint hint
                ? new HttpClientMethodCallAdornmentDataModel(hint.UriToCall)
                : null;
        }
    }
}
