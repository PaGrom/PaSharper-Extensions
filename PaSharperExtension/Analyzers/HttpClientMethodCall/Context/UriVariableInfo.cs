namespace PaSharperExtension.Analyzers.HttpClientMethodCall.Context
{
    /// <summary>
    /// Uri variable info
    /// </summary>
    public sealed class UriVariableInfo : VariableInfo<UriVariableInfo>
    {
        /// <summary>
        /// Uri string variable info
        /// </summary>
        public StringVariableInfo UriStringVariableInfo { get; set; }

        ///<inheritdoc cref="VariableInfo{T}.Clone"/>
        public override UriVariableInfo Clone()
        {
            var clone = (UriVariableInfo) MemberwiseClone();
            clone.UriStringVariableInfo = UriStringVariableInfo?.Clone();
            return clone;
        }

        ///<inheritdoc cref="VariableInfo{T}.Merge"/>
        public override UriVariableInfo Merge(UriVariableInfo info)
        {
            if (VariableName != info.VariableName)
            {
                // Can't merge 2 different variables
                return null;
            }

            var result = Clone();

            // if string variable value changed, mark it as unknown
            if (IsUnknown || info.IsUnknown || UriStringVariableInfo != info.UriStringVariableInfo)
            {
                result.IsUnknown = true;
                result.UriStringVariableInfo = null;
            }

            return result;
        }
    }
}
