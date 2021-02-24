using System.Drawing;
using JetBrains.Application.InlayHints;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB.Tree;
using JetBrains.UI.RichText;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall
{
    [DaemonIntraTextAdornmentProvider(typeof(HttpClientMethodCallAdornmentProvider))]
    [DaemonTooltipProvider(typeof(InlayHintTooltipProvider))]
    [StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.CodeInsights), AttributeId = AnalysisHighlightingAttributeIds.PARAMETER_NAME_HINT)]
    public class HttpClientMethodCallInfoHint : IInlayHintWithDescriptionHighlighting
    {
        public ITreeNode RootVariableDeclarationNode { get; }
        public ITreeNode PathVariableDeclarationNode { get; }
        public string UriToCall { get; }
        public InlayHintsMode InlayHintsMode { get; }
        public ITreeNode HttpClientMethodArgumentNode;

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
            HttpClientMethodArgumentNode = httpClientMethodArgumentNode;
        }

        public bool IsValid()
        {
            return HttpClientMethodArgumentNode.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            var offset = HttpClientMethodArgumentNode switch
            {
                ILiteralExpression _ => HttpClientMethodArgumentNode.GetDocumentRange().StartOffset + 1,
                _ => HttpClientMethodArgumentNode.GetDocumentRange().EndOffset
            };

            return new DocumentRange(offset);
        }

        public string ToolTip => HttpClientMethodArgumentNode switch
        {
            ILiteralExpression literalExpression => GetToolTip(literalExpression),
            _ => $"/* {UriToCall} */"
        };

        public string ErrorStripeToolTip { get; }
        public RichText Description => new RichText("HttpClient will invoke ")
            .Append(UriToCall, new TextStyle(FontStyle.Italic))
            .Append(" uri");

        private string GetToolTip(ILiteralExpression expression)
        {
            var value = (string)expression.ConstantValue.Value;
            return UriToCall.Remove(UriToCall.Length - value.Length);
        }
    }
}
