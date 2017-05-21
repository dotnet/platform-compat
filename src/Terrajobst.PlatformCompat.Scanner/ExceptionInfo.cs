using System;

namespace Terrajobst.PlatformCompat.Scanner
{
    public struct ExceptionInfo
    {
        public static readonly ExceptionInfo DoesNotThrow = new ExceptionInfo(-1, "");

        private ExceptionInfo(int level, string site)
        {
            Level = level;
            Site = site;
        }

        public static ExceptionInfo ThrowsAt(int level, string site)
        {
            return new ExceptionInfo(level, site);
        }

        public ExceptionInfo Combine(ExceptionInfo other)
        {
            if (!Throws)
                return other;

            if (!other.Throws)
                return this;

            return ThrowsAt(Math.Min(Level, other.Level), Level < other.Level ? Site : other.Site );
        }

        public bool Throws => Level >= 0;

        public int Level { get; }

        public string Site { get; set; }

        public override string ToString()
        {
            return Level.ToString();
        }
    }
}
