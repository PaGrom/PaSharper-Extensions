using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;

namespace PaSharperExtension.Analyzers.PossibleNullCoalescingAssignment
{
    [ElementProblemAnalyzer(typeof(IEqualityExpression), HighlightingTypes = new[] {typeof(NullCoalescingAssignmentSuggestion)})]
    public sealed class PossibleNullCoalescingAssignmentAnalyzer : ElementProblemAnalyzer<IEqualityExpression>, IConditionalElementProblemAnalyzer
    {
        private static void AddNullCoalescingAssignmentHighlightings(
            [NotNull] IHighlightingConsumer consumer,
            [NotNull] IIfStatement ifStatement,
            [NotNull] IReferenceExpression expressionAssignTo,
            [NotNull] ICSharpExpression expressionToAssign)
        {
            var highlighting = new NullCoalescingAssignmentSuggestion("Possible to change 'if' to '??='", ifStatement, expressionAssignTo, expressionToAssign);

            consumer.AddHighlighting(highlighting, ifStatement.IfKeyword.GetDocumentRange());
        }

        [Pure]
        private static bool IsLiteral([CanBeNull] IExpression expression, [NotNull] TokenNodeType tokenType)
            => (expression as ICSharpLiteralExpression)?.Literal?.GetTokenType() == tokenType;

        [Pure]
        [CanBeNull]
        private static ICSharpExpression TryGetOtherOperand(
            [NotNull] IEqualityExpression equalityExpression,
            [NotNull] TokenNodeType operandNodeType)
        {
            var (leftOperand, rightOperand) = equalityExpression;

            if (IsLiteral(rightOperand, operandNodeType))
            {
                return leftOperand;
            }

            if (IsLiteral(leftOperand, operandNodeType))
            {
                return rightOperand;
            }

            return null;
        }

        protected override void Run(IEqualityExpression equalityExpression, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (equalityExpression.EqualityType != EqualityExpressionType.EQEQ
                || !(equalityExpression.Parent is IIfStatement ifStatement)
                || ifStatement.Else != null)
            {
                return;
            }

            var expressionStatement = ifStatement.LastChild switch
            {
                IBlock block
                    when block.Statements.Count == 1
                         && block.Statements[0] is IExpressionStatement expSt => expSt,
                IExpressionStatement expSt => expSt,
                _ => null
            };

            if (!(expressionStatement?.Expression is IAssignmentExpression assignmentExpression)
                || assignmentExpression.AssignmentType != AssignmentType.EQ
                || !(assignmentExpression.Dest is IReferenceExpression referenceExpressionInsideIf))
            {
                return;
            }

            var expression = TryGetOtherOperand(equalityExpression, CSharpTokenType.NULL_KEYWORD);

            if (!(expression is IReferenceExpression referenceExpressionInsideBlock))
            {
                return;
            }

            if (referenceExpressionInsideIf.NameIdentifier.Name == referenceExpressionInsideBlock.NameIdentifier.Name)
            {
                AddNullCoalescingAssignmentHighlightings(consumer, ifStatement, referenceExpressionInsideBlock, assignmentExpression.LastChild as ICSharpExpression);
            }
        }

        public bool ShouldRun(IFile file, ElementProblemAnalyzerData data) => file.IsCSharp8Supported();
    }
}
