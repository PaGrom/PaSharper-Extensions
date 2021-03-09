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

namespace PaSharperExtension.Analyzers.CharInsteadOfInt
{
    [QuickFix]
    public sealed class CharInsteadOfIntFix : QuickFixBase
    {
        [NotNull] private readonly CharInsteadOfIntSuggestion _highlighting;

        public CharInsteadOfIntFix([NotNull] CharInsteadOfIntSuggestion highlighting) => _highlighting = highlighting;

        public override bool IsAvailable(IUserDataHolder cache) => true;

        public override string Text => "Convert to string";

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_highlighting.Argument);

                ModificationUtil.ReplaceChild(
                    _highlighting.Argument,
                    factory.CreateStatement(_highlighting.Argument.GetText().Replace('\'', '\"')));
            }

            return _ => { };
        }
    }
}