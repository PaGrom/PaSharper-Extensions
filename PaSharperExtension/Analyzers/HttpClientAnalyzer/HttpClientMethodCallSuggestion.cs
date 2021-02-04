using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.Application.UI.Controls.Utils;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;
using JetBrains.Application.InlayHints;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [RegisterConfigurableSeverity(
        SeverityId,
        null,
        HighlightingGroupIds.CodeSmell,
        "Necessary 'await' (PaSharper Extensions)",
        "Necessary to await task before instance disposing",
        Severity.SUGGESTION)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        AttributeId = AnalysisHighlightingAttributeIds.SUGGESTION,
        OverlapResolve = OverlapResolveKind.WARNING)]
    public sealed class HttpClientMethodCallSuggestion : IHighlighting
    {
        private const string SeverityId = "PaSharperExtension.HttpClientMethodCall";

        internal HttpClientMethodCallSuggestion(
            [NotNull] string message,
            [NotNull] ICSharpLiteralExpression expressionToChange)
        {
            ToolTip = message;
            ExpressionToChange = expressionToChange;
        }

        [NotNull]
        internal ICSharpLiteralExpression ExpressionToChange { get; }

        public bool IsValid() => true;

        public DocumentRange CalculateRange() => throw new NotImplementedException();

        public string ErrorStripeToolTip => ToolTip;

        [NotNull]
        public string ToolTip { get; }
    }

    [RegisterHighlighterGroup(
        HighlightAttributeGroupId,
        HighlightAttributeGroupId,
        HighlighterGroupPriority.CODE_SETTINGS)]
    [RegisterHighlighter(
        HighlightAttributeId,
        GroupId = HighlightAttributeGroupId,
        ForegroundColor = "#707070",
        BackgroundColor = "#EBEBEB",
        DarkForegroundColor = "#787878",
        DarkBackgroundColor = "#3B3B3C",
        EffectType = EffectType.INTRA_TEXT_ADORNMENT,
        Layer = 3000,
        TransmitUpdates = true,
        VSPriority = 40)]
    [DaemonIntraTextAdornmentProvider(typeof(CognitiveComplexityAdornmentProvider))]
    [DaemonTooltipProvider(typeof(InlayHintTooltipProvider))]
    [StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.CodeInsights), AttributeId = HighlightAttributeId)]
    public class CognitiveComplexityInfoHint : IInlayHintWithDescriptionHighlighting
    {
        public const string HighlightAttributeId = "Cognitive Complexity";
        public const string HighlightAttributeGroupId = HighlightAttributeId + " Group";

        private readonly DocumentOffset _offset;
        private readonly ITreeNode _node;

        public CognitiveComplexityInfoHint(ITreeNode node, DocumentOffset offset, int value)
        {
            _node = node;
            _offset = offset;
            Value = value;
        }

        public int Value { get; }

        public bool IsValid()
        {
            return _node.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return new DocumentRange(_offset);
        }

        public string ToolTip { get; } = "23";
        public string ErrorStripeToolTip { get; } = "32";
        public RichText Description { get; } = "2323";
    }

    [SolutionComponent]
    public class CognitiveComplexityAdornmentProvider : IHighlighterIntraTextAdornmentProvider
    {
        public bool IsValid(IHighlighter highlighter)
        {
            return highlighter.UserData is CognitiveComplexityInfoHint;
        }

        public IIntraTextAdornmentDataModel CreateDataModel(IHighlighter highlighter)
        {
            return highlighter.UserData is CognitiveComplexityInfoHint hint
                ? new CognitiveComplexityAdornmentDataModel(hint.Value)
                : null;
        }
    }

    public class CognitiveComplexityAdornmentDataModel : IIntraTextAdornmentDataModel
    {
        private readonly int _value;

        public CognitiveComplexityAdornmentDataModel(int value)
        {
            _value = value;
        }

        public void ExecuteNavigation(PopupWindowContextSource popupWindowContextSource)
        {
        }

        public RichText Text => $"+{_value}";
        public bool HasContextMenu { get; }
        public IPresentableItem ContextMenuTitle { get; }
        public IEnumerable<BulbMenuItem> ContextMenuItems { get; }
        public bool IsNavigable { get; }
        public TextRange? SelectionRange { get; }
        public IconId IconId { get; }
        public bool IsPreceding { get; }
        public int Order { get; }
        public InlayHintsMode InlayHintsMode => InlayHintsMode.Always;
    }
}