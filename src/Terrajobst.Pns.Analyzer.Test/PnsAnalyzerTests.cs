using Microsoft.CodeAnalysis.Diagnostics;
using Terrajobst.Pns.Analyzer.Test.Helpers;
using Xunit;

namespace Terrajobst.Pns.Analyzer.Test
{
    public class PnsAnalyzerTests : CSharpDiagnosticTest
    {
        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new PnsAnalyzer();
        }

        protected override (string name, string defaultSettings) GetAdditionalFileSettings()
        {
            return (PnsAnalyzer.SettingsName, "TargetFramework=netstandard20");
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_WhenDocumentEmpty()
        {
            AssertNoMatch(string.Empty);
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_WhenApiDefinedInSource()
        {
            var source = @"
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

            AssertNoMatch(source);
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_ForSuppressedPlatforms()
        {
            var source = @"
                using Microsoft.Win32;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            Registry.LocalMachine.{{OpenSubKey}}(string.Empty);
                        }
                    }
                }
            ";

            var expected = @"
                PNS001: RegistryKey.OpenSubKey(string) isn't supported on MacOSX
            ";

            var settings = @"
                PnsIgnore=Linux
            ";

            AssertMatch(source, expected, settings);
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_WhenAllPlatformsSuppressed()
        {
            var source = @"
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

            var settings = @"
                PnsIgnore=Linux
                MacOSX
            ";

            AssertNoMatch(source, settings);
        }

        [Fact]
        public void PnsAnalyzer_DoesNotTrigger_WhenTargetingNetFramework()
        {
            var source = @"
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

            var settings = @"
                TargetFramework=net45
            ";

            AssertNoMatch(source, settings);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForMethods()
        {
            var source = @"
                using Microsoft.Win32;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            Registry.LocalMachine.{{OpenSubKey}}(string.Empty);
                        }
                    }
                }
            ";

            var expected = @"
                PNS001: RegistryKey.OpenSubKey(string) isn't supported on Linux, MacOSX
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForConstructors()
        {
            var source = @"
                using System.Threading;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var e = {{new AutoResetEvent(false)}};
                        }
                    }
                }
            ";

            var expected = @"
                PNS001: AutoResetEvent.AutoResetEvent(bool) isn't supported on Linux, MacOSX
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForProperties()
        {
            var source = @"
                using System;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var width = Console.{{WindowWidth}};
                        }
                    }
                }
            ";

            var expected = @"
                PNS001: Console.WindowWidth isn't supported on Linux, MacOSX
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForEvents()
        {
             var source = @"
                using System;
                using System.Runtime.Serialization;

                namespace ConsoleApp1
                {
                    class MyException : Exception
                    {
                        public MyException()
                        {
                            {{SerializeObjectState}} += MyException_SerializeObjectState;
                        }

                        private void MyException_SerializeObjectState(object sender, SafeSerializationEventArgs e)
                        {
                        }
                    }
                }
            ";

            var expected = @"
                PNS001: Exception.SerializeObjectState isn't supported on Linux, MacOSX, Windows
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void PnsAnalyzer_Triggers_ForOperators()
        {
            var source = @"
                using System.Security.Cryptography;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            CngAlgorithm x;
                            CngAlgorithm y;
                            var result = {{x == y}};
                        }
                    }
                }
            ";

            var expected = @"
                PNS001: CngAlgorithm.operator ==(CngAlgorithm, CngAlgorithm) isn't supported on Linux, MacOSX
            ";

            AssertMatch(source, expected);
        }
    }
}