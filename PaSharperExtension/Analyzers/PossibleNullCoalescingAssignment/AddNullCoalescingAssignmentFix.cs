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

namespace PaSharperExtension.Analyzers.PossibleNullCoalescingAssignment
{
    [QuickFix]
    public sealed class AddNullCoalescingAssignmentFix : QuickFixBase
    {
        [NotNull] private readonly NullCoalescingAssignmentSuggestion _highlighting;

        public AddNullCoalescingAssignmentFix([NotNull] NullCoalescingAssignmentSuggestion highlighting) => _highlighting = highlighting;

        public override bool IsAvailable(IUserDataHolder cache) => true;

        public override string Text
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("Change 'if' to '??='");

                return builder.ToString();
            }
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_highlighting.IfStatement);

                // replace 'if' with '??='
                ModificationUtil.ReplaceChild(
                    _highlighting.IfStatement,
                    factory.CreateStatement($"{_highlighting.ExpressionAssignTo.GetText()} ??= {_highlighting.ExpressionToAssign.GetText()};"));
            }

            return _ => { };
        }
    }
}