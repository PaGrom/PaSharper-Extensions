using System;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace PaSharperExtension.Analyzers.PossibleNullCoalescingAssignment
{
    [RegisterConfigurableSeverity(
        SeverityId,
        null,
        HighlightingGroupIds.LanguageUsage,
        "Possible to use Null Coalescing Assignment (PaSharper Extensions)",
        "Possible to change if statement to Null Coalescing Assignment",
        Severity.SUGGESTION)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        AttributeId = AnalysisHighlightingAttributeIds.SUGGESTION,
        OverlapResolve = OverlapResolveKind.WARNING)]
    public sealed class NullCoalescingAssignmentSuggestion : IHighlighting
    {
        private const string SeverityId = "PaSharperExtension.NullCoalescingAssignment";

        internal NullCoalescingAssignmentSuggestion(
            [NotNull] string message,
            [NotNull] IIfStatement ifStatement,
            [NotNull] IReferenceExpression expressionAssignTo,
            [NotNull] ICSharpExpression expressionToAssign)
        {
            ToolTip = message;
            IfStatement = ifStatement;
            ExpressionAssignTo = expressionAssignTo;
            ExpressionToAssign = expressionToAssign;
        }

        [NotNull]
        internal IIfStatement IfStatement { get; }

        [NotNull]
        internal IReferenceExpression ExpressionAssignTo { get; }

        [NotNull]
        internal ICSharpExpression ExpressionToAssign { get; }

        public bool IsValid() => true;

        public DocumentRange CalculateRange() => throw new NotImplementedException();

        public string ErrorStripeToolTip => ToolTip;

        [NotNull]
        public string ToolTip { get; }
    }
}
