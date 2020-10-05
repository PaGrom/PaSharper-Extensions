using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace PaSharperExtension.Analyzers.NecessaryAwait
{
    [ElementProblemAnalyzer(typeof(IReturnStatement), HighlightingTypes = new[] { typeof(NecessaryAwaitSuggestion) })]
    public sealed class NecessaryAwaitAnalyzer : ElementProblemAnalyzer<IReturnStatement>
    {
        private static void AddNecessaryAwaitHighlightings(
            [NotNull] IHighlightingConsumer consumer,
            [NotNull] Action addAsync,
            [NotNull] IReturnStatement returnStatement,
            [NotNull] ICSharpExpression expressionToAwait)
        {
            var highlighting = new NecessaryAwaitSuggestion("Necessary 'await' (add 'async'/'await')", addAsync, returnStatement, expressionToAwait);

            consumer.AddHighlighting(highlighting, returnStatement.ReturnKeyword.GetDocumentRange());
        }

        protected override void Run(IReturnStatement returnStatement, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (!(returnStatement.Value is IInvocationExpression invocationExpression))
            {
                return;
            }

            var container = returnStatement.GetContainingNode<IParametersOwnerDeclaration>();

            if (container == null || !(container is IMethodDeclaration methodDeclaration))
            {
                return;
            }

            var returnType = methodDeclaration.Type;

            if (!returnType.IsTask() && !returnType.IsGenericTask())
            {
                return;
            }

            var usingStatement = returnStatement.PathToRoot()
                .Skip(1)
                .TakeWhile(node => node != container)
                .FirstOrDefault(node => node is IUsingStatement) as IUsingStatement;

            if (usingStatement?.VariableDeclarations.Count != 1)
            {
                return;
            }

            var usingVariableIdentifier = usingStatement?.VariableDeclarations.First().FirstChild as IIdentifier;

            var returnReferenceExpression = invocationExpression.FirstChild as IReferenceExpression;

            var variableIdentifier = (returnReferenceExpression.FirstChild as IReferenceExpression).FirstChild as IIdentifier;

            if (variableIdentifier.Name != usingVariableIdentifier.Name)
            {
                return;
            }

            AddNecessaryAwaitHighlightings(consumer, () => methodDeclaration.SetAsync(true), returnStatement, invocationExpression);
        }
    }
}
