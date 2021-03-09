using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.Util;

namespace PaSharperExtension.Analyzers.CharInsteadOfInt
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression), HighlightingTypes = new[] { typeof(CharInsteadOfIntSuggestion) })]
    public sealed class CharInsteadOfIntAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        private static void AddCharInsteadOfIntHighlightings(
            [NotNull] IHighlightingConsumer consumer,
            [NotNull] ICSharpArgument argument)
        {
            var highlighting = new CharInsteadOfIntSuggestion(argument);

            consumer.AddHighlighting(highlighting, argument.GetDocumentRange());
        }

        protected override void Run(IInvocationExpression expression, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            if (!(expression.InvokedExpression is IReferenceExpression referenceExpression))
            {
                return;
            }

            var resolve = referenceExpression.Reference.Resolve();
            if ((EnumPattern) resolve.Info != ResolveErrorType.OK || !(resolve.DeclaredElement is IMethod method))
            {
                return;
            }

            var parameterIndex = -1;

            for (var i = 0; i < method.Parameters.Count; i++)
            {
                var argument = expression.ArgumentList.Arguments[i];
                if (!(argument.Expression is ICSharpLiteralExpression literalExpression)
                    || literalExpression.Literal.GetTokenType() != CSharpTokenType.CHARACTER_LITERAL)
                {
                    continue;
                }

                var parameter = method.Parameters[i];

                if (!parameter.Type.IsInt() && !parameter.Type.IsLong())
                {
                    continue;
                }

                // found char to int cast
                parameterIndex = i;
            }

            if (parameterIndex == -1)
            {
                return;
            }

            var relevantMethod = GetOverloadMethodWithStringParameter(method.GetContainingType().Methods, method, parameterIndex);

            if (relevantMethod == null)
            {
                return;
            }

            AddCharInsteadOfIntHighlightings(consumer, expression.ArgumentList.Arguments[parameterIndex]);
        }

        /// <summary>
        /// Find method with the same signature, but with string instead of int parameter
        /// </summary>
        private IMethod GetOverloadMethodWithStringParameter(IEnumerable<IMethod> methods, IMethod methodWithIntParameter, int intParameterWithCharArgumentIndex)
        {
            foreach (var method in methods)
            {
                if (method.Parameters.Count != methodWithIntParameter.Parameters.Count)
                {
                    continue;
                }

                var isRelevantParameterString = false;

                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    if (i == intParameterWithCharArgumentIndex)
                    {
                        if (method.Parameters[i].Type.IsString())
                        {
                            isRelevantParameterString = true;
                            continue;
                        }
                        break;
                    }

                    if (!method.Parameters[i].Type.Equals(methodWithIntParameter.Parameters[i].Type))
                    {
                        isRelevantParameterString = false;
                        break;
                    }
                }

                if (isRelevantParameterString)
                {
                    return method;
                }
            }

            return null;
        }
    }
}
