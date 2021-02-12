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
            typeof(HttpClientMethodCallInfoHint)
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

            foreach (var httpClientMethodCallInfo in inspector.HttpClientMethodCallInfos)
            {
                consumer.AddHighlighting(new HttpClientMethodCallInfoHint(httpClientMethodCallInfo.HttpClientMethodFirstArgument,
                    httpClientMethodCallInfo.RootVariableDeclarationNode,
                    httpClientMethodCallInfo.PathVariableDeclarationNode,
                    new Uri(new Uri(httpClientMethodCallInfo.Root), httpClientMethodCallInfo.Path).AbsoluteUri));
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
        public ITreeNode VariableDeclarationNode { get; set; }

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

    public class HttpClientMethodCallInfo
    {
        public ITreeNode RootVariableDeclarationNode { get; set; }

        public string Root { get; set; }

        public ITreeNode PathVariableDeclarationNode { get; set; }

        public string Path { get; set; }

        public ICSharpExpression HttpClientMethodFirstArgument { get; set; }
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

        public List<HttpClientMethodCallInfo> HttpClientMethodCallInfos { get; } = new List<HttpClientMethodCallInfo>();

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

            var argument = invocationExpression.Arguments[0].Value;

            var stringVariableInfo = argument switch
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

            HttpClientMethodCallInfos.Add(new HttpClientMethodCallInfo
            {
                RootVariableDeclarationNode = httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableDeclarationNode,
                Root = httpClientInfo.RootUriVariableInfo.UriStringVariableInfo.VariableValue,
                PathVariableDeclarationNode = stringVariableInfo.VariableDeclarationNode,
                Path = stringVariableInfo.VariableValue,
                HttpClientMethodFirstArgument = argument
            });
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
            stringVariableInfo.VariableDeclarationNode = variableDeclaration;

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
    }
}
