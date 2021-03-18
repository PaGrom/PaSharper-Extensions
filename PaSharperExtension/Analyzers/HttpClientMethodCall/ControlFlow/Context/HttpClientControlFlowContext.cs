﻿using System.Collections.Generic;
using System.Linq;
using PaSharperExtension.Extensions;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall.ControlFlow.Context
{
    /// <summary>
    /// Control flow context for http client analyzer
    /// </summary>
    public class HttpClientControlFlowContext
    {
        /// <summary>
        /// Collection of all declared strings
        /// </summary>
        public List<StringVariableInfo> StringVariables { get; set; } = new List<StringVariableInfo>();

        /// <summary>
        /// Collection of all declared uris
        /// </summary>
        public List<UriVariableInfo> UriVariables { get; set; } = new List<UriVariableInfo>();

        /// <summary>
        /// Collection of all declared HttpClients
        /// </summary>
        public List<HttpClientVariableInfo> HttpClientInfos { get; set; } = new List<HttpClientVariableInfo>();

        /// <summary>
        /// Merges two contexts to single
        /// </summary>
        public HttpClientControlFlowContext Merge(HttpClientControlFlowContext context)
        {
            return new HttpClientControlFlowContext
            {
                StringVariables = StringVariables.MergeZip(context.StringVariables, (f, s) => f.Merge(s)).ToList(),
                UriVariables = UriVariables.MergeZip(context.UriVariables, (f, s) => f.Merge(s)).ToList(),
                HttpClientInfos = HttpClientInfos.MergeZip(context.HttpClientInfos, (f, s) => f.Merge(s)).ToList()
            };
        }
    }
}
