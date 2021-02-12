using System.Collections.Generic;
using JetBrains.Application.InlayHints;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.Application.UI.Controls.Utils;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Resources.Icons;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;
using PaSharperExtension.Analyzers.HttpClientAnalyzer.Options;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    public class HttpClientMethodCallAdornmentDataModel : IIntraTextAdornmentDataModel
    {
        private readonly Lifetime _lifetime;
        private readonly ISettingsStore _settingsStore;
        private readonly ITreeNode _rootVariableDeclarationNode;
        private readonly ITreeNode _pathVariableDeclarationNode;
        private readonly string _uriToCall;

        public HttpClientMethodCallAdornmentDataModel(
            Lifetime lifetime,
            ISettingsStore settingsStore,
            ITreeNode rootVariableDeclarationNode,
            ITreeNode pathVariableDeclarationNode,
            string uriToCall)
        {
            _lifetime = lifetime;
            _settingsStore = settingsStore;
            _rootVariableDeclarationNode = rootVariableDeclarationNode;
            _pathVariableDeclarationNode = pathVariableDeclarationNode;
            _uriToCall = uriToCall;
        }

        public void ExecuteNavigation(PopupWindowContextSource popupWindowContextSource)
        {
            // клик с контролм на хинт
        }

        public RichText Text => _uriToCall;
        public bool HasContextMenu { get; } = true; // контекстное меню по правой кнопке
        public IPresentableItem ContextMenuTitle { get; } = new PresentableItem("HttpClient Method Call Hint");

        private IEnumerable<BulbMenuItem> GetBulbMenuItems()
        {
            var menuItems = new List<BulbMenuItem>
            {
                new BulbMenuItem(new ExecutableItem(() =>
                {
                    System.Diagnostics.Process.Start("explorer.exe", Text.Text);
                }), "Open link in browser", null, BulbMenuAnchors.FirstClassContextItems),
                new BulbMenuItem(new ExecutableItem(() =>
                {
                    _rootVariableDeclarationNode.NavigateToTreeNode(true);
                }), "Navigate to root variable declaration", null, BulbMenuAnchors.FirstClassContextItems),
                new BulbMenuItem(new ExecutableItem(() =>
                {
                    _pathVariableDeclarationNode.NavigateToTreeNode(true);
                }), "Navigate to path variable declaration", null, BulbMenuAnchors.FirstClassContextItems)
            };

            menuItems.AddRange(IntraTextAdornmentDataModelHelper.CreateChangeVisibilityBulbMenuItems(_settingsStore, (HttpClientMethodCallHintSettingsKey key) => key.VisibilityMode, BulbMenuAnchors.SecondClassContextItems));

            menuItems.Add(IntraTextAdornmentDataModelHelper.CreateConfigureBulbMenuItem(HttpClientMethodCallHintOptionsPage.Pid));

            menuItems.Add(IntraTextAdornmentDataModelHelper.CreateTurnOffAllInlayHintsBulbMenuItem(_settingsStore));

            return menuItems;
        }

        public IEnumerable<BulbMenuItem> ContextMenuItems => GetBulbMenuItems();

        public bool IsNavigable { get; } = true;
        public TextRange? SelectionRange { get; } = null;
        public IconId IconId { get; } = null;
        public bool IsPreceding { get; } = false; // к чему крипить? false если символ слева от хинта
        public int Order { get; } = 1; // чем ближе к 0, тем ближе к символу
        public InlayHintsMode InlayHintsMode => _settingsStore.BindToContextLive(_lifetime, ContextRange.ApplicationWide)
            .GetValueProperty(_lifetime, (HttpClientMethodCallHintSettingsKey key) => key.VisibilityMode).GetValue();
    }
}
