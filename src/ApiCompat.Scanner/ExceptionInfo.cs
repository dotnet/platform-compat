using System;

namespace Terrajobst.PlatformCompat.Scanner
{
    public struct ExceptionInfo
    {
        public static readonly ExceptionInfo DoesNotThrow = new ExceptionInfo(-1);

        private ExceptionInfo(int level)
        {
            Level = level;
        }

        public static ExceptionInfo ThrowsAt(int level)
        {
            return new ExceptionInfo(level);
        }

        public ExceptionInfo Combine(ExceptionInfo other)
        {
            if (!Throws)
                return other;

            if (!other.Throws)
                return this;

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
