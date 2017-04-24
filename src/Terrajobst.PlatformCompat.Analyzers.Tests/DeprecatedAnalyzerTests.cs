using Microsoft.CodeAnalysis.Diagnostics;
using Terrajobst.PlatformCompat.Analyzers.Deprecated;
using Terrajobst.PlatformCompat.Analyzers.Tests.Helpers;
using Xunit;

namespace Terrajobst.PlatformCompat.Analyzers.Tests
{
    public class DeprecatedAnalyzerTests : CSharpDiagnosticTest
    {
        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DeprecatedAnalyzer();
        }

        [Fact]
        public void DeprecatedAnalyzer_DoesNotTrigger_WhenDocumentEmpty()
        {
            AssertNoMatch(string.Empty);
        }

        [Fact]
        public void DeprecatedAnalyzer_DoesNotTrigger_WhenApiDefinedInSource()
        {
            var source = @"
                namespace System.Collections
                {
                    public class ArrayList
                    {
                    }

                    class Program
                    {
                        static void Main(string[] args)
                        {
                            new ArrayList();
                        }
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void DeprecatedAnalyzer_Triggers_DE0001()
        {
            var source = @"
                using System.Security;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            {{SecureString}} x;
                        }
                    }
                }
            ";

            var expected = @"
                DE0001: SecureString is deprecated
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void DeprecatedAnalyzer_Triggers_DE0002()
        {
            var source = @"
                using System.Security.{{Permissions}};
            ";

            var expected = @"
                DE0002: Permissions is deprecated
            ";

            AssertMatch(source, expected);
        }

        [Theory]
        [InlineData("WebRequest")]
        [InlineData("FtpWebRequest")]
        [InlineData("FileWebRequest")]
        [InlineData("HttpWebRequest")]
        public void DeprecatedAnalyzer_Triggers_DE0003(string typeName)
        {
            var source = @"
                using System.Net;

                class Program
                {
                    static void Main()
                    {
                        {{$TYPE_NAME$}} x = null;
                    }
                }
            ".Replace("$TYPE_NAME$", typeName);

            var expected = $@"
                DE0003: {typeName} is deprecated
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void DeprecatedAnalyzer_Triggers_DE0004()
        {
            var source = @"
                using System.Net;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            {{WebClient}} x;
                        }
                    }
                }
            ";

            var expected = @"
                DE0004: WebClient is deprecated
            ";

            AssertMatch(source, expected);
        }
    }
}