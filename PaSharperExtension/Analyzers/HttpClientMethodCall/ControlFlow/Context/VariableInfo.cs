namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow.Context
{
    /// <summary>
    /// Abstract variable info
    /// </summary>
    public abstract class VariableInfo<T>
    {
        /// <summary>
        /// Variable name
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Mark if variable was changed in one of leafs and we are not sure about value
        /// </summary>
        public bool IsUnknown { get; set; }

        /// <summary>
        /// Clone object
        /// </summary>
        public abstract T Clone();

        /// <summary>
        /// Merge two objects to single
        /// </summary>
        public abstract T Merge(T info);
    }
}
