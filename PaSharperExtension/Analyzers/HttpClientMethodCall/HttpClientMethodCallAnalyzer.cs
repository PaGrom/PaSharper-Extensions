using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ControlFlow.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Context;
using PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Options;
using PaSharperExtension.Extensions;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall
{
    [ElementProblemAnalyzer(typeof(IInvocationExpression), HighlightingTypes = new[] {typeof(HttpClientMethodCallInfoHint)})]
    public sealed class HttpClientMethodCallAnalyzer : ElementProblemAnalyzer<IInvocationExpression>, IElementProblemAnalyzerConsumingControlFlowGraph
    {
        private readonly Lifetime _lifetime;
        private readonly ISettingsStore _settingsStore;
        private readonly List<IMethodDeclaration> _analyzedMethodDeclarations = new List<IMethodDeclaration>();
        private readonly Dictionary<IClassBody, VariablesInfoContext> _classVariablesContexts = new Dictionary<IClassBody, VariablesInfoContext>();

        public HttpClientMethodCallAnalyzer(Lifetime lifetime, ISettingsStore settingsStore)
        {
            _lifetime = lifetime;
            _settingsStore = settingsStore;
        }

        protected override void Run(IInvocationExpression invocationExpression, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var visibilityMode = _settingsStore.BindToContextLive(_lifetime, ContextRange.ApplicationWide)
                .GetValueProperty(_lifetime, (HttpClientMethodCallHintOptions key) => key.VisibilityMode).GetValue();

            if (visibilityMode == InlayHintsMode.Never)
            {
                return;
            }

            // Run analyzer just on HttpClient invocation
            if (invocationExpression.ExtensionQualifier == null
                || !invocationExpression.ExtensionQualifier.GetExpressionType().ToIType().IsHttpClient())
            {
                return;
            }

            var classBody = GetClassBody(invocationExpression);

            AnalyzeClassMembers(classBody);

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

            var factory = new HttpClientControlFlowContextFactory(_classVariablesContexts[classBody]);

            var inspector = new HttpClientControlFlowInspector(controlFlowGraph, factory);

            inspector.Inspect();

            foreach (var httpClientMethodCallInfo in inspector.HttpClientMethodCallInfos)
            {
                consumer.AddHighlighting(new HttpClientMethodCallInfoHint(httpClientMethodCallInfo.HttpClientMethodFirstArgument,
                    httpClientMethodCallInfo.BaseAddressVariableDeclarationNode,
                    httpClientMethodCallInfo.MethodAddressVariableDeclarationNode,
                    new Uri(new Uri(httpClientMethodCallInfo.BaseAddressVariableValue), httpClientMethodCallInfo.MethodAddressVariableValue).AbsoluteUri,
                    visibilityMode));
            }
        }

        private IClassBody GetClassBody([NotNull] ITreeNode node)
        {
            var rootNode = node;
            while (!(rootNode is IClassBody))
            {
                rootNode = rootNode.Parent;
            }

            return rootNode as IClassBody;
        }

        private void AnalyzeClassMembers(IClassBody classBody)
        {
            if (_classVariablesContexts.ContainsKey(classBody))
            {
                return;
            }

            var variablesProcessor = new VariablesProcessor();

            AnalyzeConstantsDeclarations(classBody, variablesProcessor);

            _classVariablesContexts[classBody] = variablesProcessor.VariablesInfoContext;
        }

        private void AnalyzeConstantsDeclarations(IClassBody classBody, VariablesProcessor variablesProcessor)
        {
            foreach (var multipleConstantDeclaration in classBody.ConstantDeclarations)
            {
                foreach (var declarator in multipleConstantDeclaration.Declarators)
                {
                    if (declarator is IConstantDeclaration constantDeclaration)
                    {
                        ProcessConstantDeclaration(constantDeclaration, variablesProcessor);
                    }
                }
            }
        }

        private void ProcessConstantDeclaration(IConstantDeclaration constantDeclaration, VariablesProcessor variablesProcessor)
        {
            if (constantDeclaration.Type.IsString())
            {
                variablesProcessor.ProcessStringConstantDeclaration(constantDeclaration);
            }
        }
    }
}
