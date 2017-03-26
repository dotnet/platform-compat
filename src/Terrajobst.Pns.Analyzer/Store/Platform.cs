using System;

namespace Terrajobst.Pns.Analyzer.Store
{
    [Flags]
    internal enum Platform
    {
        None = 0,
        Linux = 1,
        MacOSX = 2,
        Windows = 4
    }
}
