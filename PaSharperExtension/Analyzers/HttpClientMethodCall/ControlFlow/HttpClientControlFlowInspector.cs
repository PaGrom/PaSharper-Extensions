using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.ControlFlow.Impl;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Context;
using PaSharperExtension.Extensions;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow
{
    /// <summary>
    /// HttpClient control flow inspector
    /// </summary>
    public sealed class HttpClientControlFlowInspector : ControlFlowGraphInspector<VariablesInfoContext>
    {
        /// <summary>
        /// Output collection with all found HttpClient method calls
        /// </summary>
        public List<HttpClientApiMethodCallInfo> HttpClientMethodCallInfos { get; } = new List<HttpClientApiMethodCallInfo>();

        public HttpClientControlFlowInspector([NotNull] ControlFlowGraph controlFlowGraph, [NotNull] IControlFlowContextFactory<VariablesInfoContext> contextFactory)
            : base(controlFlowGraph, contextFactory)
        {
        }

        ///<inheritdoc cref="ControlFlowGraphInspector{T}.InspectLeafElementAndSetContextToExits"/>
        protected override void InspectLeafElementAndSetContextToExits(IControlFlowElement element, VariablesInfoContext variablesInfoContext)
        {
            var variablesProcessor = new VariablesProcessor(variablesInfoContext);

            if (element.SourceElement is IDeclarationStatement declarationStatement)
            {
                variablesProcessor.ProcessDeclarationStatement(declarationStatement);

                foreach (var controlFlowEdge in element.Exits)
                {
                    SetContext(controlFlowEdge, variablesInfoContext);
                }

                return;
            }

            if (element.SourceElement is IAssignmentExpression {Dest: IReferenceExpression referenceExpression} assignmentExpression)
            {
                variablesProcessor.ProcessAssignmentToReferenceExpression(assignmentExpression, referenceExpression);
            }

            if (element.SourceElement is IInvocationExpression {ExtensionQualifier: ExtensionArgumentInfo extensionArgumentInfo} invocationExpression
                && extensionArgumentInfo.GetExpressionType().ToIType().IsHttpClient()
                && extensionArgumentInfo.Expression is IReferenceExpression httpClientReferenceExpression)
            {
                var httpClientApiMethodCallInfo = ProcessHttpClientMethodInvocation(httpClientReferenceExpression, invocationExpression, variablesInfoContext);
                if (httpClientApiMethodCallInfo != null)
                {
                    HttpClientMethodCallInfos.Add(httpClientApiMethodCallInfo);
                }
            }

            foreach (var controlFlowEdge in element.Exits)
            {
                SetContext(controlFlowEdge, variablesInfoContext);
            }
        }

        /// <summary>
        /// Process HttpClient method invocation
        /// </summary>
        /// <param name="httpClientReferenceExpression">Reference expression to HttpClient object</param>
        /// <param name="invocationExpression">HttpClient method invocation expression</param>
        /// <param name="variablesInfoContext">Variables context</param>
        public HttpClientApiMethodCallInfo ProcessHttpClientMethodInvocation(IReferenceExpression httpClientReferenceExpression, IInvocationExpression invocationExpression, VariablesInfoContext variablesInfoContext)
        {
            var httpClientVariableName = httpClientReferenceExpression.NameIdentifier.Name;

            var httpClientInfo = variablesInfoContext.HttpClientInfos.SingleOrDefault(i => i.VariableName == httpClientVariableName);

            if (httpClientInfo == null)
            {
                return null;
            }

            var argument = invocationExpression.Arguments[0].Value;

            var stringVariableInfo = argument switch
            {
                ICSharpLiteralExpression literalExpression => VariablesProcessor.ProcessStringLiteralExpression(literalExpression),
                IReferenceExpression referenceExpression => variablesInfoContext.StringVariables.SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name),
                _ => null
            };

            if (stringVariableInfo == null)
            {
                return null;
            }

            httpClientInfo.MethodValues.Add(stringVariableInfo);

            if (httpClientInfo.RootUriVariableInfo?.UriStringVariableInfo.IsUnknown == false
                && !stringVariableInfo.IsUnknown)
            {
                return new HttpClientApiMethodCallInfo
                {
                    BaseAddressVariableDeclarationNode = httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableDeclarationNode,
                    BaseAddressVariableValue = httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableValue,
                    MethodAddressVariableDeclarationNode = stringVariableInfo.VariableDeclarationNode,
                    MethodAddressVariableValue = stringVariableInfo.VariableValue,
                    HttpClientMethodFirstArgument = argument
                };
            }

            return null;
        }
    }
}
