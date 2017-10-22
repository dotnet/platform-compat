namespace Microsoft.DotNet.Scanner
{
    public struct ExceptionInfo
    {
        public static readonly ExceptionInfo DoesNotThrow = new ExceptionInfo(-1, string.Empty);

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

            return Level < other.Level ? ThrowsAt(Level, Site) : ThrowsAt(other.Level, other.Site);
        }

        public bool Throws => Level >= 0;

        public int Level { get; }

        public string Site { get; }

        public override string ToString()
        {
            return Level.ToString();
        }
    }
}
