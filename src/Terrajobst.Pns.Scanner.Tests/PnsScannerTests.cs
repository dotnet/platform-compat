using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Terrajobst.Pns.Scanner.Tests
{
    public partial class PnsScannerTests
    {
        private static readonly CSharpCompilation CompilationTemplate = CSharpCompilation.Create(
            "dummy.dll",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        private static void AssertMatch(string source, string matches)
        {
            using (var host = new HostEnvironment())
            {
                var expectedDocIds = ParseLines(matches);
                var assembly = CreateAssembly(host, source);
                var actualDocIds = GetResults(assembly).Select(r => r.docId);

                Assert.Equal(expectedDocIds, actualDocIds);
            }
        }

        private static void AssertNoMatch(string source)
        {
            AssertMatch(source, string.Empty);
        }

        private static IAssembly CreateAssembly(HostEnvironment host, string text)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = CompilationTemplate.AddSyntaxTrees(syntaxTree);

            using (var memoryStream = new MemoryStream())
            {
                var result = compilation.Emit(memoryStream);
                Assert.Empty(result.Diagnostics);

                memoryStream.Position = 0;

                return host.LoadAssemblyFrom(compilation.AssemblyName, memoryStream);
            }
        }

        private static IEnumerable<(string docId, PnsResult result)> GetResults(IAssembly assembly)
        {
            var results = new List<(string docId, PnsResult result)>();
            var handler = new DelegatedPnsReporter((r, m) =>
            {
                if (r.Throws)
                    results.Add((m.DocId(), r));
            });
            var scanner = new PnsScanner(handler);
            scanner.AnalyzeAssembly(assembly);
            return results;
        }

        private static IEnumerable<string> ParseLines(string text)
        {
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                        yield return line.Trim();
                }
            }
        }

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
