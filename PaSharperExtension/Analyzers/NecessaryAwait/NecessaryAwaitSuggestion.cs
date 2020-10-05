using System;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace PaSharperExtension.Analyzers.NecessaryAwait
{
    [RegisterConfigurableSeverity(
        SeverityId,
        null,
        HighlightingGroupIds.CodeRedundancy,
        "Necessary 'await' (PaSharper Extensions)",
        "Necessary to await task before instance disposing",
        Severity.SUGGESTION)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        AttributeId = AnalysisHighlightingAttributeIds.SUGGESTION,
        OverlapResolve = OverlapResolveKind.WARNING)]
    public sealed class NecessaryAwaitSuggestion : IHighlighting
    {
        private const string SeverityId = "NecessaryAwait";

        internal NecessaryAwaitSuggestion(
            [NotNull] string message,
            [NotNull] Action addAsync,
            [NotNull] IReturnStatement returnStatement,
            [NotNull] ICSharpExpression expressionToAwait)
        {
            ToolTip = message;
            AddAsync = addAsync;
            ReturnStatement = returnStatement;
            ExpressionToAwait = expressionToAwait;
        }

        [NotNull]
        internal Action AddAsync { get; }

        [NotNull]
        internal IReturnStatement ReturnStatement { get; }

        [NotNull]
        internal ICSharpExpression ExpressionToAwait { get; }

        public bool IsValid() => true;

        public DocumentRange CalculateRange() => throw new NotImplementedException();

        public string ErrorStripeToolTip => ToolTip;

        [NotNull]
        public string ToolTip { get; }
    }
}