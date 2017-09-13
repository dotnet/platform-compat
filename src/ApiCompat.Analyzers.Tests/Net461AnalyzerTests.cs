using Microsoft.CodeAnalysis.Diagnostics;
using ApiCompat.Analyzers.Net461;
using ApiCompat.Analyzers.Tests.Helpers;
using Xunit;

namespace ApiCompat.Analyzers.Tests
{
    public class Net461AnalyzerTests : CSharpDiagnosticTest
    {
        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new Net461Analyzer();
        }

        protected override (string name, string defaultSettings) GetAdditionalFileSettings()
        {
            return (PlatformCompatOptions.SettingsName, "TargetFramework=netstandard20");
        }

        [Fact]
        public void Net461Analyzer_DoesNotTrigger_WhenDocumentEmpty()
        {
            AssertNoMatch(string.Empty);
        }

        [Fact]
        public void Net461Analyzer_DoesNotTrigger_WhenApiDefinedInSource()
        {
            var source = @"
                namespace System
                {
                    public class AppContext
                    {
                        public string GetData(string key) { return null; }
                    }

                    class Program
                    {
                        static void Main(string[] args)
                        {
                            AppContext.GetData(null);
                        }
                    }
                }
            ";

            AssertNoMatch(source);
        }

        [Theory]
        [InlineData("net461")]
        [InlineData("netcore20")]
        public void Net641Analyzer_DoesNotTrigger_WhenTargetingWrongFramework(string targetFramework)
        {
            var source = @"
                using System;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var data = AppContext.GetData(string.Empty);
                            Console.WriteLine(data);
                        }
                    }
                }
            ";

            var settings = $@"
                TargetFramework={targetFramework}
            ";

            AssertNoMatch(source, settings);
        }

        [Fact]
        public void Net641Analyzer_Triggers_ForTypes()
        {
            var source = @"
                using System.Data.Common;

                class Program
                {
                    static void Main(string[] args)
                    {
                        {{DbColumn}} x = null;
                    }
                }
            ";

            var expected = @"
                PC002: DbColumn isn't available in .NET Framework 4.6.1
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void Net641Analyzer_Triggers_ForMethods()
        {
            var source = @"
                using System;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var data = AppContext.{{GetData}}(string.Empty);
                            Console.WriteLine(data);
                        }
                    }
                }
            ";

            var expected = @"
                PC002: AppContext.GetData(string) isn't available in .NET Framework 4.6.1
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void Net641Analyzer_Triggers_ForGenericExtensionMethods()
        {
            var source = @"
                using System.Linq;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var q = Enumerable.Empty<int>().{{Append}}(1);
                        }
                    }
                }
            ";

            var expected = @"
                PC002: Enumerable.Append<TSource>(IEnumerable<TSource>, TSource) isn't available in .NET Framework 4.6.1
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void Net641Analyzer_Triggers_ForProperties()
        {
            var source = @"
                using System;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var data = AppContext.{{TargetFrameworkName}};
                            Console.WriteLine(data);
                        }
                    }
                }
            ";

            var expected = @"
                PC002: AppContext.TargetFrameworkName isn't available in .NET Framework 4.6.1
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void Net641Analyzer_Triggers_ForEvents()
        {
            // System.Linq	Enumerable	Append<TSource>(IEnumerable<TSource>, TSource)

            var source = @"
                using System.Diagnostics.Tracing;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var es = new EventSource();
                            es.{{EventCommandExecuted}} += (s, e) => { };
                        }
                    }
                }
            ";

            var expected = @"
                PC002: EventSource.EventCommandExecuted isn't available in .NET Framework 4.6.1
            ";

            AssertMatch(source, expected);
        }
    }
}