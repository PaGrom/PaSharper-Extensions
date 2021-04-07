using JetBrains.ReSharper.Psi.Tree;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.Context
{
    /// <summary>
    /// String variable info
    /// </summary>
    public sealed class StringVariableInfo : VariableInfo<StringVariableInfo>
    {
        /// <summary>
        /// Node with variable declaration
        /// </summary>
        public ITreeNode VariableDeclarationNode { get; set; }

        /// <summary>
        /// String variable value
        /// </summary>
        public string VariableValue { get; set; }

        ///<inheritdoc cref="VariableInfo{T}.Clone"/>
        public override StringVariableInfo Clone() => (StringVariableInfo) MemberwiseClone();

        ///<inheritdoc cref="VariableInfo{T}.Merge"/>
        public override StringVariableInfo Merge(StringVariableInfo info)
        {
            if (VariableDeclarationNode != info.VariableDeclarationNode)
            {
                // Can't merge 2 different vars
                return null;
            }

            var result = Clone();

            // if variable value changed, mark it as unknown
            if (IsUnknown || info.IsUnknown || VariableValue != info.VariableValue)
            {
                result.IsUnknown = true;
                result.VariableValue = null;
            }

            return result;
        }
    }
}
