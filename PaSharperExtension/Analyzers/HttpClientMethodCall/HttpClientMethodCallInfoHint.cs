using System.Drawing;
using JetBrains.Application.InlayHints;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.UI.RichText;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall
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
            string uriToCall,
            InlayHintsMode inlayHintsMode)
        {
            RootVariableDeclarationNode = rootVariableDeclarationNode;
            PathVariableDeclarationNode = pathVariableDeclarationNode;
            UriToCall = uriToCall;
            InlayHintsMode = inlayHintsMode;
            _httpClientMethodArgumentNode = httpClientMethodArgumentNode;
            _offset = httpClientMethodArgumentNode.GetDocumentRange().StartOffset;
        }

        public ITreeNode RootVariableDeclarationNode { get; }
        public ITreeNode PathVariableDeclarationNode { get; }
        public string UriToCall { get; }
        public InlayHintsMode InlayHintsMode { get; }

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
        public RichText Description => new RichText("HttpClient will invoke ")
            .Append(UriToCall, new TextStyle(FontStyle.Italic))
            .Append(" uri");
    }
}
