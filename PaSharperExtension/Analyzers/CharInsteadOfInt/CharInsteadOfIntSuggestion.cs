using System;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace PaSharperExtension.Analyzers.CharInsteadOfInt
{
    [RegisterConfigurableSeverity(
        SeverityId,
        null,
        HighlightingGroupIds.CodeSmell,
        "[PaSharper] Passed char instead of int",
        Description,
        Severity.SUGGESTION)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        AttributeId = AnalysisHighlightingAttributeIds.SUGGESTION,
        OverlapResolve = OverlapResolveKind.WARNING)]
    public sealed class CharInsteadOfIntSuggestion : IHighlighting
    {
        private const string SeverityId = "PaSharperExtension.CharInsteadOfInt";

        private const string Description = "Char is possibly unintentionally used as integer where overload with string available";

        internal CharInsteadOfIntSuggestion(
            [NotNull] ICSharpArgument argument)
        {
            Argument = argument;
        }

        [NotNull]
        internal ICSharpArgument Argument { get; }

        public bool IsValid() => true;

        public DocumentRange CalculateRange() => throw new NotImplementedException();

        public string ErrorStripeToolTip => ToolTip;

        [NotNull] public string ToolTip => Description;
    }
}