using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.ControlFlow.Impl;
using JetBrains.ReSharper.Psi.CSharp.Impl.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression),
        HighlightingTypes = new[]
        {
            //typeof(HttpClientMethodCallSuggestion),
            typeof(CognitiveComplexityInfoHint)
        })]
    public sealed class HttpClientMethodCallAnalyzer : ElementProblemAnalyzer<IInvocationExpression>, IElementProblemAnalyzerConsumingControlFlowGraph
    {
        private readonly List<IMethodDeclaration> _analyzedMethodDeclarations = new List<IMethodDeclaration>();

        protected override void Run(IInvocationExpression invocationExpression, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            // Run analyzer just on HttpClient invocation
            if (invocationExpression.ExtensionQualifier == null
                || !invocationExpression.ExtensionQualifier.GetExpressionType().ToIType().IsHttpClient())
            {
                return;
            }

            // Get current method
            var container = invocationExpression.GetContainingNode<IParametersOwnerDeclaration>();

            if (!(container is IMethodDeclaration methodDeclaration) || _analyzedMethodDeclarations.Contains(methodDeclaration))
            {
                return;
            }

            _analyzedMethodDeclarations.Add(methodDeclaration);

            var controlFlowGraph = (ControlFlowGraph) data.GetOrBuildControlFlowGraph(methodDeclaration);

            if (controlFlowGraph == null)
            {
                return;
            }

            var factory = new HttpClientContextFactory();

            var inspector = new HttpClientInspector(controlFlowGraph, factory);

            inspector.Inspect();

            foreach (var possibleWrongHttpClientMethodCall in inspector.PossibleWrongMethodCalls)
            {
                var highlighting = new HttpClientMethodCallSuggestion("Trim '/' from start " +
                                                                      $"{(string.IsNullOrWhiteSpace(possibleWrongHttpClientMethodCall.MethodVariableToFixName) ? "" : $"of variable '{possibleWrongHttpClientMethodCall.MethodVariableToFixName}' value ")}" +
                                                                      $"to call '{possibleWrongHttpClientMethodCall.UriAfterFix}' " +
                                                                      $"instead of '{possibleWrongHttpClientMethodCall.UriBeforeFix}'",
                    possibleWrongHttpClientMethodCall.VariableToChangeExpression);

                //consumer.AddHighlighting(highlighting, possibleWrongHttpClientMethodCall.MethodInvocationExpression.GetDocumentRange());

                consumer.AddHighlighting(new CognitiveComplexityInfoHint(possibleWrongHttpClientMethodCall.VariableToChangeExpression, possibleWrongHttpClientMethodCall.VariableToChangeExpression.GetDocumentRange().EndOffset, 1));
            }
        }
    }

    public class HttpClientAnalyzerContext
    {
        public List<StringVariableInfo> StringVariables { get; set; } = new List<StringVariableInfo>();

        public List<UriVariableInfo> UriVariables { get; set; } = new List<UriVariableInfo>();

        public List<HttpClientVariableInfo> HttpClientInfos { get; set; } = new List<HttpClientVariableInfo>();
    }

    public class StringVariableInfo
    {
        public string VariableName { get; set; }

        public string VariableValue { get; set; }

        public ICSharpLiteralExpression RootLiteralExpression { get; set; }
    }

    public class UriVariableInfo
    {
        public string VariableName { get; set; }

        public StringVariableInfo UriStringVariableInfo { get; set; }
    }

    public class HttpClientVariableInfo
    {
        public string HttpClientVariableName { get; set; }

        public UriVariableInfo RootUriVariableInfo { get; set; }

        public List<StringVariableInfo> MethodValues { get; set; } = new List<StringVariableInfo>();
    }

    public class PossibleWrongHttpClientMethodCall
    {
        public string UriBeforeFix { get; set; }

        public string UriAfterFix { get; set; }

        public ICSharpLiteralExpression VariableToChangeExpression { get; set; }

        public string MethodVariableToFixName { get; set; }

        public IInvocationExpression MethodInvocationExpression { get; set; }
    }

    public class HttpClientContextFactory : IControlFlowContextFactory<HttpClientAnalyzerContext>
    {
        public HttpClientAnalyzerContext CloneContext(HttpClientAnalyzerContext analyzerContext)
        {
            return analyzerContext;
        }

        public HttpClientAnalyzerContext Merge(IList<HttpClientAnalyzerContext> contexts)
        {
            return contexts.First();
        }

        public HttpClientAnalyzerContext InitialContext { get; } = new HttpClientAnalyzerContext();
    }

    public class HttpClientInspector : ControlFlowGraphInspector<HttpClientAnalyzerContext>
    {
        private const string HttpClientBaseAddressPropertyName = nameof(HttpClient.BaseAddress);

        public List<PossibleWrongHttpClientMethodCall> PossibleWrongMethodCalls { get; } = new List<PossibleWrongHttpClientMethodCall>();

        public HttpClientInspector([NotNull] ControlFlowGraph controlFlowGraph, [NotNull] IControlFlowContextFactory<HttpClientAnalyzerContext> contextFactory) : base(controlFlowGraph, contextFactory)
        {
        }

        protected override void InspectLeafElementAndSetContextToExits(IControlFlowElement element, HttpClientAnalyzerContext analyzerContext)
        {
            if (element.SourceElement is IDeclarationStatement declarationStatement)
            {
                ProcessDeclarationStatement(analyzerContext, declarationStatement);

                //foreach (var controlFlowEdge in element.Exits)
                //{
                //    SetContext(controlFlowEdge, analyzerContext);
                //}

                return;
            }

            if (element.SourceElement is IAssignmentExpression {Dest: IReferenceExpression referenceExpression} assignmentExpression)
            {
                ProcessAssignmentToReferenceExpression(assignmentExpression, referenceExpression, analyzerContext);
            }

            if (element.SourceElement is IInvocationExpression {ExtensionQualifier: { }} invocationExpression
                && invocationExpression.ExtensionQualifier is ExtensionArgumentInfo extensionArgumentInfo
                && extensionArgumentInfo.GetExpressionType().ToIType().IsHttpClient()
                && extensionArgumentInfo.Expression is IReferenceExpression httpClientReferenceExpression)
            {
                ProcessHttpClientMethodInvocation(httpClientReferenceExpression, invocationExpression, analyzerContext);
            }

            //foreach (var controlFlowEdge in element.Exits)
            //{
            //    SetContext(controlFlowEdge, analyzerContext);
            //}
        }

        private void ProcessHttpClientMethodInvocation(IReferenceExpression httpClientReferenceExpression, IInvocationExpression invocationExpression, HttpClientAnalyzerContext analyzerContext)
        {
            var httpClientVariableName = httpClientReferenceExpression.NameIdentifier.Name;

            var httpClientInfo = analyzerContext.HttpClientInfos.SingleOrDefault(i => i.HttpClientVariableName == httpClientVariableName);

            if (httpClientInfo == null)
            {
                return;
            }

            var stringVariableInfo = invocationExpression.Arguments[0].Value switch
            {
                ICSharpLiteralExpression literalExpression => ProcessStringLiteralExpression(literalExpression),
                IReferenceExpression referenceExpression => analyzerContext.StringVariables.SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name),
                _ => null
            };

            if (stringVariableInfo == null)
            {
                return;
            }

            httpClientInfo.MethodValues.Add(stringVariableInfo);

            var (_, pathPart) = ParseRootUri(httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableValue);

            if (stringVariableInfo.VariableValue.StartsWith("/")
                && !string.IsNullOrWhiteSpace(pathPart))
            {
                PossibleWrongMethodCalls.Add(new PossibleWrongHttpClientMethodCall
                {
                    UriBeforeFix = new Uri(new Uri(httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableValue), stringVariableInfo.VariableValue).AbsoluteUri,
                    UriAfterFix = new Uri(new Uri(httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableValue), stringVariableInfo.VariableValue.TrimStart('/')).AbsoluteUri,
                    VariableToChangeExpression = stringVariableInfo.RootLiteralExpression,
                    MethodVariableToFixName = stringVariableInfo.VariableName,
                    MethodInvocationExpression = invocationExpression
                });
            }
        }

        private static void ProcessAssignmentToReferenceExpression(IAssignmentExpression assignmentExpression, IReferenceExpression referenceExpression, HttpClientAnalyzerContext analyzerContext)
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

                var httpClientInfo = analyzerContext.HttpClientInfos
                    .SingleOrDefault(i => i.HttpClientVariableName == httpClientVariableName);

                if (httpClientInfo == null)
                {
                    return;
                }

                httpClientInfo.RootUriVariableInfo = assignmentExpression.Source switch
                {
                    IObjectCreationExpression objectCreationExpression => ProcessUriCreationExpression(objectCreationExpression, analyzerContext),
                    IReferenceExpression uriReferenceExpression => analyzerContext.UriVariables.SingleOrDefault(v => v.VariableName == uriReferenceExpression.NameIdentifier.Name),
                    _ => null
                };

                return;
            }

            if (assignmentExpression.GetExpressionType().ToIType().IsString())
            {
                var variableInfo = analyzerContext.StringVariables
                    .SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name);

                if (variableInfo != null)
                {
                    switch (assignmentExpression.AssignmentType)
                    {
                        case AssignmentType.PLUSEQ:
                            variableInfo.VariableValue += (string) assignmentExpression.Source.ConstantValue.Value;
                            break;
                    }

                    //foreach (var controlFlowEdge in element.Exits)
                    //{
                    //    SetContext(controlFlowEdge, analyzerContext);
                    //}
                    return;
                }
            }
        }

        private static void ProcessDeclarationStatement(HttpClientAnalyzerContext analyzerContext, IDeclarationStatement declarationStatement)
        {
            foreach (var variableDeclaration in declarationStatement.VariableDeclarations)
            {
                if (variableDeclaration.Type.IsString())
                {
                    ProcessStringVariableDeclaration(variableDeclaration, analyzerContext);
                    continue;
                }

                if (variableDeclaration.Type.IsUri())
                {
                    ProcessUriVariableDeclaration(variableDeclaration, analyzerContext);
                    continue;
                }

                if (variableDeclaration.Type.IsHttpClient())
                {
                    ProcessHttpClientVariableDeclaration(variableDeclaration, analyzerContext);
                    continue;
                }
            }
        }

        /// <summary>
        /// Process string variable declaration
        /// </summary>
        private static StringVariableInfo ProcessStringVariableDeclaration(ILocalVariableDeclaration variableDeclaration, HttpClientAnalyzerContext analyzerContext)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer {Value: ICSharpLiteralExpression literalExpression}))
            {
                return null;
            }

            var stringVariableInfo = ProcessStringLiteralExpression(literalExpression);
            stringVariableInfo.VariableName = variableDeclaration.DeclaredName;

            analyzerContext.StringVariables.Add(stringVariableInfo);

            return stringVariableInfo;
        }

        /// <summary>
        /// Process string literal expression
        /// </summary>
        private static StringVariableInfo ProcessStringLiteralExpression(ICSharpLiteralExpression literalExpression)
        {
            var stringVariableInfo = new StringVariableInfo
            {
                VariableValue = (string) literalExpression.ConstantValue.Value,
                RootLiteralExpression = literalExpression
            };

            return stringVariableInfo;
        }

        /// <summary>
        /// Process Uri variable declaration
        /// </summary>
        private static UriVariableInfo ProcessUriVariableDeclaration(ILocalVariableDeclaration variableDeclaration, HttpClientAnalyzerContext analyzerContext)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer {Value: IObjectCreationExpression uriCreationExpression})
                || uriCreationExpression.Arguments.Count != 1)
            {
                return null;
            }

            var uriVariableInfo = ProcessUriCreationExpression(uriCreationExpression, analyzerContext);

            if (uriVariableInfo == null)
            {
                return null;
            }

            uriVariableInfo.VariableName = variableDeclaration.DeclaredName;

            analyzerContext.UriVariables.Add(uriVariableInfo);

            return uriVariableInfo;
        }

        private static UriVariableInfo ProcessUriCreationExpression(IObjectCreationExpression uriCreationExpression, HttpClientAnalyzerContext analyzerContext)
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
                IReferenceExpression referenceExpression => analyzerContext.StringVariables
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
        private static HttpClientVariableInfo ProcessHttpClientVariableDeclaration(ILocalVariableDeclaration variableDeclaration, HttpClientAnalyzerContext analyzerContext)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer expressionInitializer))
            {
                return null;
            }

            var varName = variableDeclaration.DeclaredName;

            var httpClientInfo = new HttpClientVariableInfo
            {
                HttpClientVariableName = varName
            };

            analyzerContext.HttpClientInfos.Add(httpClientInfo);

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

            httpClientInfo.RootUriVariableInfo = ProcessUriCreationExpression(baseAddressUriCreationExpression, analyzerContext);

            return httpClientInfo;
        }

        /// <summary>
        /// Split url to parts (general and path parts)
        /// </summary>
        private static (string generalPart, string pathPart) ParseRootUri(string rootUri)
        {
            var rootUriParts = rootUri.Split(new[] {"//"}, StringSplitOptions.None);

            var (protocol, generalWithoutProtocol) = rootUriParts.Length switch
            {
                2 => (rootUriParts[0] + "//", rootUriParts[1]),
                _ => (string.Empty, rootUriParts[0])
            };

            rootUriParts = generalWithoutProtocol.Split(new[] {'/'}, StringSplitOptions.None);

            return (protocol + rootUriParts[0], string.Join("/", rootUri.Skip(1)));
        }
    }
}
