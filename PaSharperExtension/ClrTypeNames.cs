using System;
using System.Net.Http;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;

namespace PaSharperExtension
{
    // ReSharper disable AssignNullToNotNullAttribute
    public static class ClrTypeNames
    {
        [NotNull]
        public static readonly IClrTypeName HttpClient = new ClrTypeName(typeof(HttpClient).FullName);

        [NotNull]
        public static readonly IClrTypeName Uri = new ClrTypeName(typeof(Uri).FullName);
    }
}
