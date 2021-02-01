using System;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [RegisterConfigurableSeverity(
        SeverityId,
        null,
        HighlightingGroupIds.CodeSmell,
        "Necessary 'await' (PaSharper Extensions)",
        "Necessary to await task before instance disposing",
        Severity.SUGGESTION)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        AttributeId = AnalysisHighlightingAttributeIds.SUGGESTION,
        OverlapResolve = OverlapResolveKind.WARNING)]
    public sealed class HttpClientMethodCallSuggestion : IHighlighting
    {
        private const string SeverityId = "PaSharperExtension.HttpClientMethodCall";

        internal HttpClientMethodCallSuggestion(
            [NotNull] string message,
            [NotNull] ICSharpLiteralExpression expressionToChange)
        {
            ToolTip = message;
            ExpressionToChange = expressionToChange;
        }

        [NotNull]
        internal ICSharpLiteralExpression ExpressionToChange { get; }

        public bool IsValid() => true;

        public DocumentRange CalculateRange() => throw new NotImplementedException();

        public string ErrorStripeToolTip => ToolTip;

        [NotNull]
        public string ToolTip { get; }
    }
}