using Terrajobst.Pns.Scanner.Tests.Helpers;
using Xunit;

namespace Terrajobst.Pns.Scanner.Tests
{
    public partial class PnsScannerTests : PnsTests
    {
        [Fact]
        public void PnsScanner_DoesNotDetect_Privates()
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
        public void PnsScanner_DoesNotDetect_Construction()
        {
            var source = @"
                using System;

                public class C
                {
                    public Exception PNS()
                    {
                        return new PlatformNotSupportedException();
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InConstructor()
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

            var docIds = @"
                M:C.#ctor
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InMethod()
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

            var docIds = @"
                M:C.M
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InProperty()
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

            var docIds = @"
                P:C.P
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InProperty_Getter()
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

            var docIds = @"
                P:C.P
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InProperty_Setter()
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

            var docIds = @"
                P:C.P
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InEvent()
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

            var docIds = @"
                E:C.E
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InEvent_Adder()
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

            var docIds = @"
                E:C.E
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_InEvent_Remover()
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

            var docIds = @"
                E:C.E
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_DirectThrow_ViaFactory()
        {
            var source = @"
                using System;

                public class C
                {
                    public void M()
                    {
                        throw PNS();
                    }

                    private Exception PNS()
                    {
                        return new PlatformNotSupportedException();
                    }
                }
            ";

            var docIds = @"
                M:C.M
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_IndirectThrow_Level1()
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

            var docIds = @"
                M:C.M1
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_IndirectThrow_Level2()
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

            var docIds = @"
                M:C.M1
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_Detects_IndirectThrow_Level3()
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

            var docIds = @"
                M:C.M1
            ";

            AssertMatch(source, docIds);
        }

        [Fact]
        public void PnsScanner_DoesNotDetect_IndirectThrow_Level4()
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
        public void PnsScanner_Detects_IndirectThrow_ViaFactory()
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
                        throw PNS();
                    }

                    private Exception PNS()
                    {
                        return new PlatformNotSupportedException();
                    }
                }
            ";

            var docIds = @"
                M:C.M1
            ";

            AssertMatch(source, docIds);
        }
    }
}
