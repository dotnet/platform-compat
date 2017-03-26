using System;

namespace Terrajobst.PlatformNotSupported.Analysis
{
    public struct ExceptionResult
    {
        public static readonly ExceptionResult DoesNotThrow = new ExceptionResult(-1);

        private ExceptionResult(int level)
        {
            Level = level;
        }

        public static ExceptionResult ThrowsAt(int level)
        {
            return new ExceptionResult(level);
        }

        public ExceptionResult Combine(ExceptionResult other)
        {
            if (!Throws)
                return other;

            return ThrowsAt(Math.Min(Level, other.Level));
        }

        public bool Throws => Level >= 0;

        public int Level { get; }

        public override string ToString()
        {
            return Level.ToString();
        }
    }
}
