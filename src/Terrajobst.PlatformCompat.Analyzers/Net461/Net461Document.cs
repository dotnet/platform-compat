using Terrajobst.PlatformCompat.Analyzers.Store;

namespace Terrajobst.PlatformCompat.Analyzers.Net461
{
    internal static partial class Net461Document
    {
        public static ApiStore<string> Parse(string data)
        {
            var parser = new Parser();
            return parser.Parse(data);
        }
    }
}
