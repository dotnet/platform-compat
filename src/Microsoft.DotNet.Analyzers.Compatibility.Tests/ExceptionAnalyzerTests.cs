using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.DotNet.Analyzers.Compatibility.Exceptions;
using Microsoft.DotNet.Analyzers.Compatibility.Tests.Helpers;
using Xunit;

namespace Microsoft.DotNet.Analyzers.Compatibility.Tests
{
    public class ExceptionAnalyzerTests : CSharpDiagnosticTest
    {
        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ExceptionAnalyzer();
        }

        protected override (string name, string defaultSettings) GetAdditionalFileSettings()
        {
            return (PlatformCompatOptions.SettingsName, "TargetFramework=netstandard20");
        }

        [Fact]
        public void ExceptionAnalyzer_DoesNotTrigger_WhenDocumentEmpty()
        {
            AssertNoMatch(string.Empty);
        }

        [Fact]
        public void ExceptionAnalyzer_DoesNotTrigger_WhenApiDefinedInSource()
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
        public void ExceptionAnalyzer_DoesNotTrigger_ForSuppressedPlatforms()
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
                PC001: RegistryKey.OpenSubKey(string) isn't supported on macOS
            ";

            var settings = @"
                PlatformCompatIgnore=Linux
            ";

            AssertMatch(source, expected, settings);
        }

        [Fact]
        public void ExceptionAnalyzer_DoesNotTrigger_WhenAllPlatformsSuppressed()
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
                PlatformCompatIgnore=Linux
                macOS
            ";

            AssertNoMatch(source, settings);
        }

        [Fact]
        public void ExceptionAnalyzer_DoesNotTrigger_WhenTargetingNetFramework()
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
        public void ExceptionAnalyzer_Triggers_ForMethods()
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
                PC001: RegistryKey.OpenSubKey(string) isn't supported on Linux and macOS
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void ExceptionAnalyzer_Triggers_ForConstructors()
        {
            var source = @"
                using System.IO.Pipes;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var e = {{new NamedPipeClientStream(""SomeName"")}};
                        }
                    }
                }
            ";

            var expected = @"
                PC001: NamedPipeClientStream.NamedPipeClientStream(string) isn't supported on Linux and macOS
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void ExceptionAnalyzer_Triggers_ForPropertySetter()
        {
            var source = @"
                using System;

                namespace ConsoleApp1
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            Console.{{WindowWidth}} = 30;
                        }
                    }
                }
            ";

            var expected = @"
                PC001: Console.WindowWidth.set isn't supported on Linux and macOS
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void ExceptionAnalyzer_DoesNotTrigger_ForPropertyGetter()
        {
            var source = @"
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

            var expected = string.Empty;

            AssertMatch(source, expected);
        }

        [Fact]
        public void ExceptionAnalyzer_Triggers_ForEvents()
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
                            {{SerializeObjectState += MyException_SerializeObjectState}};
                        }

                        ~MyException()
                        {
                            {{SerializeObjectState -= MyException_SerializeObjectState}};
                        }

                        private void MyException_SerializeObjectState(object sender, SafeSerializationEventArgs e)
                        {
                        }
                    }
                }
            ";

            var expected = @"
                PC001: Exception.SerializeObjectState.add isn't supported on Linux, macOS, and Windows
                PC001: Exception.SerializeObjectState.remove isn't supported on Linux, macOS, and Windows
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void ExceptionAnalyzer_Triggers_ForOperators()
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
                PC001: CngAlgorithm.operator ==(CngAlgorithm, CngAlgorithm) isn't supported on Linux and macOS
            ";

            AssertMatch(source, expected);
        }
    }
}