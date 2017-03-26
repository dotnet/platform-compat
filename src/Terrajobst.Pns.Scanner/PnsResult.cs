using System;

namespace Terrajobst.Pns.Scanner
{
    public struct PnsResult
    {
        public static readonly PnsResult DoesNotThrow = new PnsResult(-1);

        private PnsResult(int level)
        {
            Level = level;
        }

        public static PnsResult ThrowsAt(int level)
        {
            return new PnsResult(level);
        }

        public PnsResult Combine(PnsResult other)
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
