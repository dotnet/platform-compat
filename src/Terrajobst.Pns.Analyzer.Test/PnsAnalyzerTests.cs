using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Terrajobst.Pns.Analyzer.Test.Helpers;
using Xunit;

namespace Terrajobst.Pns.Analyzer.Test
{
    public class PnsAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PnsAnalyzer();
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_WhenDocumentEmpty()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_WhenApiDefinedInSource()
        {
            var test = @"
                namespace Microsoft.Win32
                {
                    public class RegistryKey
                    {
                        public RegistryKey OpenSubKey(string key) { return null; }
                    }

                    public class Registry
                    {
                        public static RegistryKey LocalMachine;
                    }

                    class Program
                    {
                        static void Main(string[] args)
                        {
                            Registry.LocalMachine.OpenSubKey(string.Empty);
                        }
                    }
                }
            ";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForMethods()
        {
            var test = @"
                using Microsoft.Win32;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            Registry.LocalMachine.OpenSubKey(string.Empty);
                        }
                    }
                }
            ";

            var expected = new DiagnosticResult
            {
                Id = "PNS001",
                Message = String.Format("RegistryKey.OpenSubKey(string) isn't supported on {0}", "Linux, MacOSX"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new [] {
                    new DiagnosticResultLocation("Test0.cs", 10, 51)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForConstructors()
        {
            var test = @"
                using System.Threading;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var e = new AutoResetEvent(false);
                        }
                    }
                }
            ";

            var expected = new DiagnosticResult
            {
                Id = "PNS001",
                Message = String.Format("AutoResetEvent.AutoResetEvent(bool) isn't supported on {0}", "Linux, MacOSX"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 10, 37)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForProperties()
        {
            var test = @"
                using System;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var width = Console.WindowWidth;
                        }
                    }
                }
            ";

            var expected = new DiagnosticResult
            {
                Id = "PNS001",
                Message = String.Format("Console.WindowWidth isn't supported on {0}", "Linux, MacOSX"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 10, 49)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForEvents()
        {
             var test = @"
                using System;
                using System.Runtime.Serialization;

                namespace ConsoleApp1
                {
                    class MyException : Exception
                    {
                        public MyException()
                        {
                            SerializeObjectState += MyException_SerializeObjectState;
                        }

                        private void MyException_SerializeObjectState(object sender, SafeSerializationEventArgs e)
                        {
                        }
                    }
                }
            ";

            var expected = new DiagnosticResult
            {
                Id = "PNS001",
                Message = String.Format("Exception.SerializeObjectState isn't supported on {0}", "Linux, MacOSX, Windows"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 11, 29)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForOperators()
        {
            var test = @"
                using System.Security.Cryptography;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            CngAlgorithm x;
                            CngAlgorithm y;
                            var result = x == y;
                        }
                    }
                }
            ";

            var expected = new DiagnosticResult
            {
                Id = "PNS001",
                Message = String.Format("CngAlgorithm.operator ==(CngAlgorithm, CngAlgorithm) isn't supported on {0}", "Linux, MacOSX"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 12, 42)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}