using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.Format;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Context;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow
{
    /// <summary>
    /// Control flow context factory
    /// </summary>
    public class HttpClientControlFlowContextFactory : IControlFlowContextFactory<VariablesInfoContext>
    {
        public HttpClientControlFlowContextFactory(VariablesInfoContext variablesInfoContext = null)
        {
            InitialContext = variablesInfoContext ?? new VariablesInfoContext();
        }

        ///<inheritdoc cref="IControlFlowContextFactory{T}.CloneContext"/>
        public VariablesInfoContext CloneContext(VariablesInfoContext methodCallContext)
        {
            return new VariablesInfoContext
            {
                HttpClientInfos = methodCallContext.HttpClientInfos.DeepCopyList(i => i.Clone()),
                StringVariables = methodCallContext.StringVariables.DeepCopyList(v => v.Clone()),
                UriVariables = methodCallContext.UriVariables.DeepCopyList(v => v.Clone())
            };
        }

        ///<inheritdoc cref="IControlFlowContextFactory{T}.Merge"/>
        public VariablesInfoContext Merge(IList<VariablesInfoContext> contexts)
            => contexts.Aggregate((f, s) => f.Merge(s));

        ///<inheritdoc cref="IControlFlowContextFactory{T}.InitialContext"/>
        public VariablesInfoContext InitialContext { get; }
    }
}
