using Microsoft.CodeAnalysis.Diagnostics;
using ApiCompat.Analyzers.ModernSdk;
using ApiCompat.Analyzers.Tests.Helpers;
using Xunit;

namespace ApiCompat.Analyzers.Tests
{
    public class ModernSdkAnalyzerTests : CSharpDiagnosticTest
    {
        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ModernSdkAnalyzer();
        }

        protected override (string name, string defaultSettings) GetAdditionalFileSettings()
        {
            return (PlatformCompatOptions.SettingsName, "TargetFramework=netstandard20");
        }

        [Fact]
        public void ModernSdk_DoesNotTrigger_WhenDocumentEmpty()
        {
            AssertNoMatch(string.Empty);
        }

        [Theory]
        [InlineData("net461")]
        [InlineData("netcore20")]
        public void ModernSdk_DoesNotTrigger_WhenTargetingWrongFramework(string targetFramework)
        {
            var source = @"
                using System.Runtime.InteropServices;

                class NativeMethods
                {
                    [DllImport(""Kernel32.dll"")]
                    static extern int GetWindowsVersion();
                }
            ";

            var settings = $@"
                TargetFramework={targetFramework}
            ";

            AssertNoMatch(source, settings);
        }

        [Fact]
        public void ModernSdk_DoesNotTrigger_WhenInModernSdk()
        {
            var source = @"
                using System.Runtime.InteropServices;

                class NativeMethods
                {
                    [DllImport(""Kernel32.dll"")]
                    static extern int CancelWaitableTimer();
                }
            ";

            AssertNoMatch(source);
        }

        [Fact]
        public void ModernSdk_Triggers_ImplicitEntryPoint()
        {
            var source = @"
                using System.Runtime.InteropServices;

                class NativeMethods
                {
                    [DllImport(""Kernel32.dll"")]
                    static extern int {{GetWindowsVersion}}();
                }
            ";

            var expected = @"
                PC003: Kernel32.dll!GetWindowsVersion isn't available in UWP
            ";

            AssertMatch(source, expected);
        }

        [Fact]
        public void ModernSdk_Triggers_WhenExplicitEntryPoint()
        {
            var source = @"
                using System.Runtime.InteropServices;

                class NativeMethods
                {
                    [DllImport(""Kernel32.dll"", EntryPoint = ""GetWindowsVersion"")]
                    static extern int {{CancelWaitableTimer}}();
                }
            ";

            var expected = @"
                PC003: Kernel32.dll!GetWindowsVersion isn't available in UWP
            ";

            AssertMatch(source, expected);
        }
    }
}