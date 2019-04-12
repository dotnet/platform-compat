using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.DotNet.Analyzers.Compatibility.BeginInvokeEndInvoke;
using Microsoft.DotNet.Analyzers.Compatibility.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.DotNet.Analyzers.Compatibility.Tests
{
    public class BeginInvokeEndInvokeAnalyzerTests : CSharpDiagnosticTest
    {
        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new BeginInvokeEndInvokeAnalyzer();
        }

        [Fact]
        public void DoesNotTrigger_WhenDocumentEmpty()
        {
            AssertNoMatch(string.Empty);
        }

        [Fact]
        public void DoesNotTriggerForCallsToOtherMethods()
        {
            var source = @"
                delegate void TestDelegate(int i);

                class Program
                {
                    static void TestMethod2(int i) {}
                    static void BeginInvoke() {}

                    static void TestMethod(int i)
                    {
                        TestDelegate t = TestMethod;
                        Program.BeginInvoke();
                        Program.TestMethod2(1);
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void DoesNotFailWhenNamespaceMissing()
        {
            var source = @"
                class BaseClass {}

                class Program : BaseClass
                {
                    static void BeginInvoke() {}

                    static void TestMethod(int i)
                    {
                        Program.BeginInvoke();
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void TriggersForBeginInvoke()
        {
            var source = @"
                delegate void TestDelegate(int i);

                class Program
                {
                    static void TestMethod(int i)
                    {
                        TestDelegate t = TestMethod;
                        var result = {{t.BeginInvoke(5, null, null)}};
                    }
                }
            ";

            var expected = "PC004: BeginInvoke is unsupported on .NET Core. Use Tasks instead.";

            AssertMatch(source, expected);
        }

        [Fact]
        public void TriggersForEndInvoke()
        {
            var source = @"
                delegate void TestDelegate(int i);

                class Program
                {
                    static void TestMethod(int i)
                    {
                        TestDelegate t = TestMethod;
                        var result = {{t.BeginInvoke(5, null, null)}};
                        {{t.EndInvoke(result)}};
                    }
                }
            ";

            var expected = @"
                            PC004: BeginInvoke is unsupported on .NET Core. Use Tasks instead.
                            PC004: EndInvoke is unsupported on .NET Core. Use Tasks instead.
                            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void TriggersForFuzzyMatches()
        {
            var source = @"
                delegate void TestDelegate(int i);

                class Program
                {
                    static void TestMethod(int i)
                    {
                        TestDelegate t = TestMethod;
                        var result = {{t.BeginInvoke()}};
                        {{t.EndInvoke(foo)}};
                    }
                }
            ";

            var expected = @"
                            PC004: BeginInvoke is unsupported on .NET Core. Use Tasks instead.
                            PC004: EndInvoke is unsupported on .NET Core. Use Tasks instead.
                            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void DoesNotTriggerForNonDelegateFuzzyMatches()
        {
            var source = @"
                delegate void TestDelegate(int i);

                class Program
                {
                    static void TestMethod(int i)
                    {
                        var t = GetObject();
                        var result = t.BeginInvoke();
                        t.EndInvoke(foo);
                    }
                }
            ";

            AssertNoMatch(source);
        }
    }
}
