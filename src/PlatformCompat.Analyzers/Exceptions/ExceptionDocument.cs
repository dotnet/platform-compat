namespace PlatformCompat.Analyzers.Store
{
    internal static partial class ExceptionDocument
    {
        public static ApiStore<Platform> Parse(string data)
        {
            var parser = new Parser();
            return parser.Parse(data);
        }
    }
}
