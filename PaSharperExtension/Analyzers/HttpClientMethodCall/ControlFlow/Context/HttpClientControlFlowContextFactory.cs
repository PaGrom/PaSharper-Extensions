using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.ControlFlow;
using JetBrains.ReSharper.Psi.Format;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow.Context
{
    /// <summary>
    /// Control flow context factory
    /// </summary>
    public class HttpClientControlFlowContextFactory : IControlFlowContextFactory<HttpClientControlFlowContext>
    {
        ///<inheritdoc cref="IControlFlowContextFactory{T}.CloneContext"/>
        public HttpClientControlFlowContext CloneContext(HttpClientControlFlowContext controlFlowContext)
        {
            return new HttpClientControlFlowContext
            {
                HttpClientInfos = controlFlowContext.HttpClientInfos.DeepCopyList(i => i.Clone()),
                StringVariables = controlFlowContext.StringVariables.DeepCopyList(v => v.Clone()),
                UriVariables = controlFlowContext.UriVariables.DeepCopyList(v => v.Clone())
            };
        }

        ///<inheritdoc cref="IControlFlowContextFactory{T}.Merge"/>
        public HttpClientControlFlowContext Merge(IList<HttpClientControlFlowContext> contexts)
            => contexts.Aggregate((f, s) => f.Merge(s));

        ///<inheritdoc cref="IControlFlowContextFactory{T}.InitialContext"/>
        public HttpClientControlFlowContext InitialContext { get; } = new HttpClientControlFlowContext();
    }
}
