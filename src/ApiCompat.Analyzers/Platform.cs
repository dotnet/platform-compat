using System;

namespace ApiCompat.Analyzers
{
    [Flags]
    public enum Platform
    {
        None = 0,
        Linux = 1,
        MacOSX = 2,
        Windows = 4
    }
}
