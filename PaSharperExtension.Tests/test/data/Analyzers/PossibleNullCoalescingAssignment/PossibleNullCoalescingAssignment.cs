using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PaSharperExtension.Tests.test.data.Analyzers.PossibleNullCoalescingAssignment
{
    public class PossibleNullCoalescingAssignment
    {
        void Foo(List<string> list)
        {
            if (list == null)
            {
                list = new List<string>();
            }
        }
    }
}
