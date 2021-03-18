using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.Format;
using PaSharperExtension.Extensions;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow.Context
{
    /// <summary>
    /// HttpClient variable info
    /// </summary>
    public sealed class HttpClientVariableInfo : VariableInfo<HttpClientVariableInfo>
    {
        /// <summary>
        /// BaseAddressVariableValue uri variable info
        /// </summary>
        public UriVariableInfo RootUriVariableInfo { get; set; }

        /// <summary>
        /// Collection of all string method variables
        /// </summary>
        public List<StringVariableInfo> MethodValues { get; set; } = new List<StringVariableInfo>();

        ///<inheritdoc cref="VariableInfo{T}.Clone"/>
        public override HttpClientVariableInfo Clone()
        {
            var clone = (HttpClientVariableInfo) MemberwiseClone();
            clone.RootUriVariableInfo = RootUriVariableInfo?.Clone();
            clone.MethodValues = MethodValues.DeepCopyList(v => v.Clone());
            return clone;
        }

        ///<inheritdoc cref="VariableInfo{T}.Merge"/>
        public override HttpClientVariableInfo Merge(HttpClientVariableInfo info)
        {
            if (VariableName != info.VariableName)
            {
                // Can't merge 2 different variables
                return null;
            }

            var result = Clone();

            // if string variable value changed, mark it as unknown
            if (IsUnknown || info.IsUnknown || RootUriVariableInfo != info.RootUriVariableInfo)
            {
                result.IsUnknown = true;
                result.RootUriVariableInfo = null;
                return result;
            }

            result.MethodValues = MethodValues.MergeZip(info.MethodValues, (f, s) => f.Merge(s)).ToList();

            return result;
        }
    }
}
