using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

namespace PaSharperExtension.Analyzers.HttpClientAnalyzer
{
    [QuickFix]
    public sealed class HttpClientMethodCallFix : QuickFixBase
    {
        [NotNull] private readonly HttpClientMethodCallSuggestion _highlighting;

        public HttpClientMethodCallFix([NotNull] HttpClientMethodCallSuggestion highlighting) => _highlighting = highlighting;

        public override bool IsAvailable(IUserDataHolder cache) => true;

        public override string Text => "Trim '/' from start of method address";

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_highlighting.ExpressionToChange);

                var value = _highlighting.ExpressionToChange.GetText();

                var indexOfFirstSlash = value.IndexOf('/');

                // trim '/' from start
                ModificationUtil.ReplaceChild(
                    _highlighting.ExpressionToChange,
                    factory.CreateStatement(value.Remove(indexOfFirstSlash, 1)));
            }

            return _ => { };
        }
    }
}