using Microsoft.Cci;
using Terrajobst.PlatformNotSupported.Analysis;

namespace pns_gen
{
    internal sealed class PnsReporter : IPlatformNotSupportedReporter
    {
        private readonly PnsDatabase _database;
        private readonly string _platform;

        public PnsReporter(PnsDatabase database, string platform)
        {
            _database = database;
            _platform = platform;
        }

        public void Report(ExceptionResult result, ITypeDefinitionMember member)
        {
            if (result.Throws)
                _database.Add(member, _platform);
        }
    }
}
