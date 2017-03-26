using System;

namespace Terrajobst.Pns.Analyzer.Test.Helpers
{
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");

            if (column < -1)
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");

            Path = path;
            Line = line;
            Column = column;
        }

        public string Path { get; }
        public int Line { get; }
        public int Column { get; }
    }
}