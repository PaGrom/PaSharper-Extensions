using System.Collections.Generic;
using JetBrains.Application.InlayHints;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.Application.UI.Controls.Utils;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    public class HttpClientMethodCallAdornmentDataModel : IIntraTextAdornmentDataModel
    {
        private readonly string _uriToCall;

        public HttpClientMethodCallAdornmentDataModel(string uriToCall)
        {
            _uriToCall = uriToCall;
        }

        public void ExecuteNavigation(PopupWindowContextSource popupWindowContextSource)
        {
            // клик с контролм на хинт
        }

        public RichText Text => _uriToCall;
        public bool HasContextMenu { get; } = false; // контекстное меню по правой кнопке
        public IPresentableItem ContextMenuTitle { get; } = null;
        public IEnumerable<BulbMenuItem> ContextMenuItems { get; } = null;
        public bool IsNavigable { get; } = false;
        public TextRange? SelectionRange { get; } = null;
        public IconId IconId { get; } = null;
        public bool IsPreceding { get; } = false; // к чему крипить? false если символ слева от хинта
        public int Order { get; } = 1; // чем ближе к 0, тем ближе к символу
        public InlayHintsMode InlayHintsMode => InlayHintsMode.Always;
    }
}
