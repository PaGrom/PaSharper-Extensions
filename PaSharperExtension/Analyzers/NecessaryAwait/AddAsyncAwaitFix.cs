using System;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

namespace PaSharperExtension.Analyzers.NecessaryAwait
{
    [QuickFix]
    public sealed class AddAsyncAwaitFix : QuickFixBase
    {
        [NotNull] private readonly NecessaryAwaitSuggestion _highlighting;

        public AddAsyncAwaitFix([NotNull] NecessaryAwaitSuggestion highlighting) => _highlighting = highlighting;

        public override bool IsAvailable(IUserDataHolder cache) => true;

        public override string Text
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("Add 'async'/'await'");

                return builder.ToString();
            }
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_highlighting.ReturnStatement);

                // add 'async'
                _highlighting.AddAsync();

                // replace 'return' with 'await'
                ModificationUtil.ReplaceChild(
                    _highlighting.ReturnStatement,
                    factory.CreateStatement($"await {_highlighting.ExpressionToAwait.GetText()};"));
            }

            return _ => { };
        }
    }
}