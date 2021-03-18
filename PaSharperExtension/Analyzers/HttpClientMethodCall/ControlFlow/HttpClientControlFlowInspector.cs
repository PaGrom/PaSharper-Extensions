using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.ControlFlow.Impl;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow.Context;
using PaSharperExtension.Extensions;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow
{
    /// <summary>
    /// HttpClient control flow inspector
    /// </summary>
    public sealed class HttpClientControlFlowInspector : ControlFlowGraphInspector<HttpClientControlFlowContext>
    {
        /// <summary>
        /// "BaseAddress" property name to search base address declaration
        /// </summary>
        private const string HttpClientBaseAddressPropertyName = nameof(HttpClient.BaseAddress);

        /// <summary>
        /// Output collection with all found HttpClient method calls
        /// </summary>
        public List<HttpClientApiMethodCallInfo> HttpClientMethodCallInfos { get; } = new List<HttpClientApiMethodCallInfo>();

        public HttpClientControlFlowInspector([NotNull] ControlFlowGraph controlFlowGraph, [NotNull] IControlFlowContextFactory<HttpClientControlFlowContext> contextFactory)
            : base(controlFlowGraph, contextFactory) { }

        ///<inheritdoc cref="ControlFlowGraphInspector{T}.InspectLeafElementAndSetContextToExits"/>
        protected override void InspectLeafElementAndSetContextToExits(IControlFlowElement element, HttpClientControlFlowContext controlFlowContext)
        {
            if (element.SourceElement is IDeclarationStatement declarationStatement)
            {
                ProcessDeclarationStatement(controlFlowContext, declarationStatement);

                foreach (var controlFlowEdge in element.Exits)
                {
                    SetContext(controlFlowEdge, controlFlowContext);
                }

                return;
            }

            if (element.SourceElement is IAssignmentExpression {Dest: IReferenceExpression referenceExpression} assignmentExpression)
            {
                ProcessAssignmentToReferenceExpression(assignmentExpression, referenceExpression, controlFlowContext);
            }

            if (element.SourceElement is IInvocationExpression {ExtensionQualifier: { }} invocationExpression
                && invocationExpression.ExtensionQualifier is ExtensionArgumentInfo extensionArgumentInfo
                && extensionArgumentInfo.GetExpressionType().ToIType().IsHttpClient()
                && extensionArgumentInfo.Expression is IReferenceExpression httpClientReferenceExpression)
            {
                ProcessHttpClientMethodInvocation(httpClientReferenceExpression, invocationExpression, controlFlowContext);
            }

            foreach (var controlFlowEdge in element.Exits)
            {
                SetContext(controlFlowEdge, controlFlowContext);
            }
        }

        /// <summary>
        /// Process HttpClient method invocation
        /// </summary>
        /// <param name="httpClientReferenceExpression">Reference expression to HttpClient object</param>
        /// <param name="invocationExpression">HttpClient method invocation expression</param>
        /// <param name="controlFlowContext">Context</param>
        private void ProcessHttpClientMethodInvocation(IReferenceExpression httpClientReferenceExpression, IInvocationExpression invocationExpression, HttpClientControlFlowContext controlFlowContext)
        {
            var httpClientVariableName = httpClientReferenceExpression.NameIdentifier.Name;

            var httpClientInfo = controlFlowContext.HttpClientInfos.SingleOrDefault(i => i.VariableName == httpClientVariableName);

            if (httpClientInfo == null)
            {
                return;
            }

            var argument = invocationExpression.Arguments[0].Value;

            var stringVariableInfo = argument switch
            {
                ICSharpLiteralExpression literalExpression => ProcessStringLiteralExpression(literalExpression),
                IReferenceExpression referenceExpression => controlFlowContext.StringVariables.SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name),
                _ => null
            };

            if (stringVariableInfo == null)
            {
                return;
            }

            httpClientInfo.MethodValues.Add(stringVariableInfo);

            if (httpClientInfo.RootUriVariableInfo?.UriStringVariableInfo.IsUnknown == false
                && !stringVariableInfo.IsUnknown)
            {
                HttpClientMethodCallInfos.Add(new HttpClientApiMethodCallInfo
                {
                    BaseAddressVariableDeclarationNode = httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableDeclarationNode,
                    BaseAddressVariableValue = httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableValue,
                    MethodAddressVariableDeclarationNode = stringVariableInfo.VariableDeclarationNode,
                    MethodAddressVariableValue = stringVariableInfo.VariableValue,
                    HttpClientMethodFirstArgument = argument
                });
            }
        }

        /// <summary>
        /// Process assignment to reference
        /// </summary>
        /// <param name="assignmentExpression">Assignment expression</param>
        /// <param name="referenceExpression">Reference expression</param>
        /// <param name="controlFlowContext">Context</param>
        private static void ProcessAssignmentToReferenceExpression(IAssignmentExpression assignmentExpression, IReferenceExpression referenceExpression, HttpClientControlFlowContext controlFlowContext)
        {
            if (assignmentExpression.GetExpressionType().ToIType().IsUri()
                && referenceExpression.ConditionalQualifier is IReferenceExpression httpClientExpression
                && httpClientExpression.GetExpressionType().ToIType().IsHttpClient())
            {
                if (!(assignmentExpression.Dest is IReferenceExpression httpClientPropertyReferenceExpression))
                {
                    return;
                }

                if (httpClientPropertyReferenceExpression.NameIdentifier.Name != HttpClientBaseAddressPropertyName)
                {
                    return;
                }

                var httpClientVariableName = httpClientExpression.NameIdentifier.Name;

                var httpClientInfo = controlFlowContext.HttpClientInfos
                    .SingleOrDefault(i => i.VariableName == httpClientVariableName);

                if (httpClientInfo == null)
                {
                    return;
                }

                httpClientInfo.RootUriVariableInfo = assignmentExpression.Source switch
                {
                    IObjectCreationExpression objectCreationExpression => ProcessUriCreationExpression(objectCreationExpression, controlFlowContext),
                    IReferenceExpression uriReferenceExpression => controlFlowContext.UriVariables.SingleOrDefault(v => v.VariableName == uriReferenceExpression.NameIdentifier.Name),
                    _ => null
                };

                return;
            }

            if (assignmentExpression.GetExpressionType().ToIType().IsString())
            {
                var variableInfo = controlFlowContext.StringVariables
                    .SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name);

                if (variableInfo != null)
                {
                    switch (assignmentExpression.AssignmentType)
                    {
                        case AssignmentType.PLUSEQ:
                            if (!variableInfo.IsUnknown)
                            {
                                variableInfo.VariableValue += (string) assignmentExpression.Source.ConstantValue.Value;
                            }

                            break;
                        case AssignmentType.EQ:
                            variableInfo.VariableValue = (string) assignmentExpression.Source.ConstantValue.Value;
                            variableInfo.IsUnknown = false;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Process declaration
        /// </summary>
        private static void ProcessDeclarationStatement(HttpClientControlFlowContext controlFlowContext, IDeclarationStatement declarationStatement)
        {
            foreach (var variableDeclaration in declarationStatement.VariableDeclarations)
            {
                if (variableDeclaration.Type.IsString())
                {
                    ProcessStringVariableDeclaration(variableDeclaration, controlFlowContext);
                    continue;
                }

                if (variableDeclaration.Type.IsUri())
                {
                    ProcessUriVariableDeclaration(variableDeclaration, controlFlowContext);
                    continue;
                }

                if (variableDeclaration.Type.IsHttpClient())
                {
                    ProcessHttpClientVariableDeclaration(variableDeclaration, controlFlowContext);
                    continue;
                }
            }
        }

        /// <summary>
        /// Process string variable declaration
        /// </summary>
        private static StringVariableInfo ProcessStringVariableDeclaration(ILocalVariableDeclaration variableDeclaration, HttpClientControlFlowContext controlFlowContext)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer {Value: ICSharpLiteralExpression literalExpression}))
            {
                return null;
            }

            var stringVariableInfo = ProcessStringLiteralExpression(literalExpression);
            stringVariableInfo.VariableName = variableDeclaration.DeclaredName;
            stringVariableInfo.VariableDeclarationNode = variableDeclaration;

            controlFlowContext.StringVariables.Add(stringVariableInfo);

            return stringVariableInfo;
        }

        /// <summary>
        /// Process string literal expression
        /// </summary>
        private static StringVariableInfo ProcessStringLiteralExpression(ICSharpLiteralExpression literalExpression)
        {
            var stringVariableInfo = new StringVariableInfo
            {
                VariableValue = (string) literalExpression.ConstantValue.Value
            };

            return stringVariableInfo;
        }

        /// <summary>
        /// Process Uri variable declaration
        /// </summary>
        private static UriVariableInfo ProcessUriVariableDeclaration(ILocalVariableDeclaration variableDeclaration, HttpClientControlFlowContext controlFlowContext)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer {Value: IObjectCreationExpression uriCreationExpression})
                || uriCreationExpression.Arguments.Count != 1)
            {
                return null;
            }

            var uriVariableInfo = ProcessUriCreationExpression(uriCreationExpression, controlFlowContext);

            if (uriVariableInfo == null)
            {
                return null;
            }

            uriVariableInfo.VariableName = variableDeclaration.DeclaredName;

            controlFlowContext.UriVariables.Add(uriVariableInfo);

            return uriVariableInfo;
        }

        /// <summary>
        /// Process Uri creation
        /// </summary>
        private static UriVariableInfo ProcessUriCreationExpression(IObjectCreationExpression uriCreationExpression, HttpClientControlFlowContext controlFlowContext)
        {
            if (!uriCreationExpression.GetExpressionType().ToIType().IsUri()
                || uriCreationExpression.Arguments.Count != 1)
            {
                return null;
            }

            var uriCreationArgument = uriCreationExpression.Arguments[0];

            var uriStringVariableInfo = uriCreationArgument.Expression switch
            {
                ICSharpLiteralExpression cSharpLiteralExpression => ProcessStringLiteralExpression(cSharpLiteralExpression),
                IReferenceExpression referenceExpression => controlFlowContext.StringVariables
                    .SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name),
                _ => null
            };

            if (uriStringVariableInfo == null)
            {
                return null;
            }

            var uriVariableInfo = new UriVariableInfo
            {
                UriStringVariableInfo = uriStringVariableInfo
            };

            return uriVariableInfo;
        }

        /// <summary>
        /// Process HttpClient variable declaration
        /// </summary>
        private static HttpClientVariableInfo ProcessHttpClientVariableDeclaration(ILocalVariableDeclaration variableDeclaration, HttpClientControlFlowContext controlFlowContext)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer expressionInitializer))
            {
                return null;
            }

            var varName = variableDeclaration.DeclaredName;

            var httpClientInfo = new HttpClientVariableInfo
            {
                VariableName = varName
            };

            controlFlowContext.HttpClientInfos.Add(httpClientInfo);

            // Check if inline initialization exists
            if (!(expressionInitializer.Value is IObjectCreationExpression
            {
                Initializer: IObjectInitializer objectInitializer
            }) || !objectInitializer.MemberInitializers.Any())
            {
                return null;
            }

            // Check if BaseAddress is setting up during init
            var baseAddressPropertyInit = objectInitializer.MemberInitializers
                .OfType<IPropertyInitializer>()
                .SingleOrDefault(i => i.MemberName.Equals(HttpClientBaseAddressPropertyName));

            // HttpClient() { BaseAddress = new Uri(baseAddress)}
            //TODO: save all Uri variable initialization
            if (!(baseAddressPropertyInit?.Expression is IObjectCreationExpression baseAddressUriCreationExpression)
                || !baseAddressUriCreationExpression.GetExpressionType().ToIType().IsUri())
            {
                return null;
            }

            httpClientInfo.RootUriVariableInfo = ProcessUriCreationExpression(baseAddressUriCreationExpression, controlFlowContext);

            return httpClientInfo;
        }
    }
}
