﻿using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PaSharperExtension.Tests.test.data.Analyzers.CharInsteadOfInt
{
    public class CharInsteadOfInt
    {
        void Method()
        {
            A.Foo({caret}'/')};
        }
    }

    public class A
    {
        public static void Foo(string a) { }
        public static void Foo(int a) { }
    }
}
