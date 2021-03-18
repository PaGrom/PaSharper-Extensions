using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow
{
    /// <summary>
    /// Info about HttpClient api method call
    /// </summary>
    public sealed class HttpClientApiMethodCallInfo
    {
        /// <summary>
        /// Node with base address variable declaration
        /// </summary>
        public ITreeNode BaseAddressVariableDeclarationNode { get; set; }

        /// <summary>
        /// Base address variable value
        /// </summary>
        public string BaseAddressVariableValue { get; set; }

        /// <summary>
        /// Node with method address variable declaration
        /// </summary>
        public ITreeNode MethodAddressVariableDeclarationNode { get; set; }

        /// <summary>
        /// Method address variable value
        /// </summary>
        public string MethodAddressVariableValue { get; set; }

        /// <summary>
        /// Reference to method first argument to add hints
        /// </summary>
        public ICSharpExpression HttpClientMethodFirstArgument { get; set; }
    }
}
