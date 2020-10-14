using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaSharperExtension.Tests.test.data.Analyzers.PossibleNullCoalescingAssignmentQuickFixes
{
    class PossibleNullCoalescingAssignmentQuickFixes
    {
        void Foo(List<string> list)
        {
            if{caret} (list == null)
                list = new List<string>();
        }
    }
}
