using System;
using System.Collections.Generic;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.ControlFlow.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow;
using PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow.Context;
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

            var factory = new HttpClientControlFlowContextFactory();

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
    }
}
