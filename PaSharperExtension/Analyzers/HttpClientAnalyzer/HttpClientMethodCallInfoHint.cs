using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.UI.RichText;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [DaemonIntraTextAdornmentProvider(typeof(HttpClientMethodCallAdornmentProvider))]
    [DaemonTooltipProvider(typeof(InlayHintTooltipProvider))]
    [StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.CodeInsights), AttributeId = AnalysisHighlightingAttributeIds.PARAMETER_NAME_HINT)]
    public class HttpClientMethodCallInfoHint : IInlayHintWithDescriptionHighlighting
    {
        private readonly DocumentOffset _offset;
        private readonly ITreeNode _httpClientMethodArgumentNode;

        public HttpClientMethodCallInfoHint(ITreeNode httpClientMethodArgumentNode,
            ITreeNode rootVariableDeclarationNode,
            ITreeNode pathVariableDeclarationNode,
            string uriToCall)
        {
            RootVariableDeclarationNode = rootVariableDeclarationNode;
            PathVariableDeclarationNode = pathVariableDeclarationNode;
            UriToCall = uriToCall;
            _httpClientMethodArgumentNode = httpClientMethodArgumentNode;
            _offset = httpClientMethodArgumentNode.GetDocumentRange().EndOffset;
        }

        public ITreeNode RootVariableDeclarationNode { get; }
        public ITreeNode PathVariableDeclarationNode { get; }
        public string UriToCall { get; }

        public bool IsValid()
        {
            return _httpClientMethodArgumentNode.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return new DocumentRange(_offset);
        }

        public string ToolTip => UriToCall;
        public string ErrorStripeToolTip { get; }
        public RichText Description { get; }
    }
}
