using Microsoft.DotNet.Scanner.Tests.Helpers;
using Xunit;

namespace Microsoft.DotNet.Scanner.Tests
{
    public partial class ExceptionScannerTests : PlatformCompatTests
    {
        [Fact]
        public void ExceptionScanner_DoesNotDetect_Privates()
        {
            var source = @"
                using System;

                internal class C
                {
                    public C()
                    {
                        throw new PlatformNotSupportedException();
                    }

                    public void M()
                    {
                        throw new PlatformNotSupportedException();
                    }

                    public int P
                    {
                        get { throw new PlatformNotSupportedException(); }
                        set { throw new PlatformNotSupportedException(); }
                    }

                    public event EventHandler E
                    {
                        add { throw new PlatformNotSupportedException(); }
                        remove { throw new PlatformNotSupportedException(); }
                    }
                }

                public class D
                {
                    private D()
                    {
                        throw new PlatformNotSupportedException();
                    }

                    private void M()
                    {
                        throw new PlatformNotSupportedException();
                    }

                    private int P
                    {
                        get { throw new PlatformNotSupportedException(); }
                        set { throw new PlatformNotSupportedException(); }
                    }

                    private event EventHandler E
                    {
                        add { throw new PlatformNotSupportedException(); }
                        remove { throw new PlatformNotSupportedException(); }
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void ExceptionScanner_DoesNotDetect_Construction()
        {
            var source = @"
                using System;

                public class C
                {
                    public Exception Create()
                    {
                        return new PlatformNotSupportedException();
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InConstructor()
        {
            var source = @"
                using System;

                public class C
                {
                    public C()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.#ctor 0 M:C.#ctor
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InMethod()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M 0 M:C.M
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InProperty()
        {
            var source = @"
                using System;

                public class C
                {
                    public int P
                    {
                        get { throw new PlatformNotSupportedException(); }
                        set { throw new PlatformNotSupportedException(); }
                    }
                }
            ";

            var expectedResults = @"
                M:C.set_P(System.Int32) 0 M:C.set_P(System.Int32)
                M:C.get_P 0 M:C.get_P
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InProperty_Getter()
        {
            var source = @"
                using System;

                public class C
                {
                    public int P
                    {
                        get { throw new PlatformNotSupportedException(); }
                        set { }
                    }
                }
            ";

            var expectedResults = @"
                M:C.get_P 0 M:C.get_P
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InProperty_Setter()
        {
            var source = @"
                using System;

                public class C
                {
                    public int P
                    {
                        get { return 0; }
                        set { throw new PlatformNotSupportedException(); }
                    }
                }
            ";

            var expectedResults = @"
                M:C.set_P(System.Int32) 0 M:C.set_P(System.Int32)
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InEvent()
        {
            var source = @"
                using System;

                public class C
                {
                    public event EventHandler E
                    {
                        add { throw new PlatformNotSupportedException(); }
                        remove { throw new PlatformNotSupportedException(); }
                    }
                }
            ";

            var expectedResults = @"
                M:C.add_E(System.EventHandler) 0 M:C.add_E(System.EventHandler)
                M:C.remove_E(System.EventHandler) 0 M:C.remove_E(System.EventHandler)
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InEvent_Adder()
        {
            var source = @"
                using System;

                public class C
                {
                    public event EventHandler E
                    {
                        add { throw new PlatformNotSupportedException(); }
                        remove { }
                    }
                }
            ";

            var expectedResults = @"
                M:C.add_E(System.EventHandler) 0 M:C.add_E(System.EventHandler)
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_InEvent_Remover()
        {
            var source = @"
                using System;

                public class C
                {
                    public event EventHandler E
                    {
                        add { }
                        remove { throw new PlatformNotSupportedException(); }
                    }
                }
            ";

            var expectedResults = @"
                M:C.remove_E(System.EventHandler) 0 M:C.remove_E(System.EventHandler)
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_DirectThrow_ViaFactory()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M()
                    {
                        throw Create();
                    }

                    private Exception Create()
                    {
                        return new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M 0 M:C.M 
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_IndirectThrow_Level1()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M1 1 M:C.M2
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_IndirectThrow_Level2()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        M3();
                    }

                    private void M3()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M1 2 M:C.M3
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_IndirectThrow_Level3()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        M3();
                    }

                    private void M3()
                    {
                        M4();
                    }

                    private void M4()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M1 3 M:C.M4
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_DoesNotDetect_IndirectThrow_Level4()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        M3();
                    }

                    private void M3()
                    {
                        M4();
                    }

                    private void M4()
                    {
                        M5();
                    }

                    private void M5()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void ExceptionScanner_Detects_IndirectThrow_ViaFactory()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        throw Create();
                    }

                    private Exception Create()
                    {
                        return new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M1 1 M:C.M2
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_ShortesPath_InMethod()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M1()
                    {
                        O1();
                        O2();
                    }

                    public void M2()
                    {
                        O1();
                        throw new PlatformNotSupportedException();
                        O2();
                    }

                    private void O1()
                    {
                        O2();
                    }

                    private void O2()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.M1 1 M:C.O2
                M:C.M2 0 M:C.M2
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_ShortesPath_InProperty()
        {
            var source = @"
                using System;

                public class C
                {
                    public int P1
                    {
                        get { M1(); throw new PlatformNotSupportedException(); }
                        set { M1(); }
                    }

                    public int P2
                    {
                        get { M1(); return -1; }
                        set { M2(); }
                    }

                    private void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.get_P1 0 M:C.get_P1
                M:C.get_P2 2 M:C.M2
                M:C.set_P1(System.Int32) 2 M:C.M2
                M:C.set_P2(System.Int32) 1 M:C.M2
            ";

            AssertMatch(source, expectedResults);
        }

        [Fact]
        public void ExceptionScanner_Detects_ShortesPath_InEvent()
        {
            var source = @"
                using System;

                public class C
                {
                    public event EventHandler E1
                    {
                        add { M1(); throw new PlatformNotSupportedException(); }
                        remove { M1(); }
                    }

                    public event EventHandler E2
                    {
                        add { M1(); }
                        remove { M2(); }
                    }

                    private void M1()
                    {
                        M2();
                    }

                    private void M2()
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            ";

            var expectedResults = @"
                M:C.add_E1(System.EventHandler) 0 M:C.add_E1(System.EventHandler)
                M:C.add_E2(System.EventHandler) 2 M:C.M2
                M:C.remove_E1(System.EventHandler) 2 M:C.M2
                M:C.remove_E2(System.EventHandler) 1 M:C.M2
            ";

            AssertMatch(source, expectedResults);
        }
    }
}
