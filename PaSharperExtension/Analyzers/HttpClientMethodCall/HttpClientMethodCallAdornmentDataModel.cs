using System.Collections.Generic;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.Application.UI.Controls.Utils;
using JetBrains.Application.UI.Icons.FeaturesIntellisenseThemedIcons;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Options;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall
{
    /// <summary>
    /// Inlay hint data model
    /// </summary>
    public class HttpClientMethodCallAdornmentDataModel : IIntraTextAdornmentDataModel
    {
        private readonly ISettingsStore _settingsStore;
        private readonly ITreeNode _rootVariableDeclarationNode;
        private readonly ITreeNode _pathVariableDeclarationNode;
        private readonly string _uriToCall;

        public HttpClientMethodCallAdornmentDataModel(
            ISettingsStore settingsStore,
            ITreeNode rootVariableDeclarationNode,
            ITreeNode pathVariableDeclarationNode,
            string uriToCall,
            InlayHintsMode inlayHintsMode)
        {
            _settingsStore = settingsStore;
            _rootVariableDeclarationNode = rootVariableDeclarationNode;
            _pathVariableDeclarationNode = pathVariableDeclarationNode;
            _uriToCall = uriToCall;
            InlayHintsMode = inlayHintsMode;
        }

        // клик с контролм на хинт
        /// <summary>
        /// Action executes after Crtl+LClick on inlay hint
        /// Enabled if <see cref="IsNavigable"/> is true
        /// </summary>
        public void ExecuteNavigation(PopupWindowContextSource popupWindowContextSource)
        {
            BrowseLink();
        }

        /// <summary>
        /// Inlay Hint Text
        /// </summary>
        public RichText Text => _uriToCall;

        /// <summary>
        /// Turn on context menu on RClick on inlay hint
        /// </summary>
        public bool HasContextMenu => true; // контекстное меню по правой кнопке

        /// <summary>
        /// Context menu title
        /// </summary>
        public IPresentableItem ContextMenuTitle { get; } = new PresentableItem("HttpClient Method Call Hint");

        /// <summary>
        /// Context menu options
        /// </summary>
        public IEnumerable<BulbMenuItem> ContextMenuItems => GetBulbMenuItems();

        /// <summary>
        /// If true <see cref="ExecuteNavigation"/> will be invoked on Ctrl+LClick
        /// </summary>
        public bool IsNavigable => true; // можно ли Ctrl+click

        /// <summary>
        /// TODO:
        /// </summary>
        public TextRange? SelectionRange => null;

        /// <summary>
        /// Inlay Hint icon (on left side)
        /// </summary>
        public IconId IconId => JetBrains.Application.UI.Icons.CommonThemedIcons.CommonThemedIcons.Move.Id;

        /// <summary>
        /// If true inlay hint will on right side of tree node, if else - left side
        /// </summary>
        public bool IsPreceding => true;

        /// <summary>
        /// Order among the other inlay hints. The closer to 0, the closer to tree node
        /// </summary>
        public int Order => 1;

        /// <summary>
        /// Inlay hint display mode
        /// </summary>
        public InlayHintsMode InlayHintsMode { get; }

        /// <summary>
        /// Configure menu items
        /// </summary>
        private IEnumerable<BulbMenuItem> GetBulbMenuItems()
        {
            var menuItems = new List<BulbMenuItem>
            {
                new BulbMenuItem(new ExecutableItem(BrowseLink),
                    "Open link in browser", null, BulbMenuAnchors.FirstClassContextItems),
                new BulbMenuItem(new ExecutableItem(() =>
                {
                    _rootVariableDeclarationNode.NavigateToTreeNode(true);
                }), "Navigate to root variable declaration", null, BulbMenuAnchors.FirstClassContextItems),
                new BulbMenuItem(new ExecutableItem(() =>
                {
                    _pathVariableDeclarationNode.NavigateToTreeNode(true);
                }), "Navigate to path variable declaration", null, BulbMenuAnchors.FirstClassContextItems)
            };

            menuItems.AddRange(IntraTextAdornmentDataModelHelper.CreateChangeVisibilityBulbMenuItems(_settingsStore, (HttpClientMethodCallHintOptions key) => key.VisibilityMode, BulbMenuAnchors.SecondClassContextItems));

            menuItems.Add(IntraTextAdornmentDataModelHelper.CreateConfigureBulbMenuItem(HttpClientMethodCallHintOptionsPage.Pid));

            menuItems.Add(IntraTextAdornmentDataModelHelper.CreateTurnOffAllInlayHintsBulbMenuItem(_settingsStore));

            return menuItems;
        }

        /// <summary>
        /// Open url in browser
        /// </summary>
        private void BrowseLink() => System.Diagnostics.Process.Start("explorer.exe", Text.Text);
    }
}
