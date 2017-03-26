using Microsoft.Cci;
using Terrajobst.Pns.Scanner;

namespace pns_gen
{
    internal sealed class PnsReporter : IPnsReporter
    {
        private readonly PnsDatabase _database;
        private readonly string _platform;

        public PnsReporter(PnsDatabase database, string platform)
        {
            _database = database;
            _platform = platform;
        }

        public void Report(PnsResult result, ITypeDefinitionMember member)
        {
            if (result.Throws)
                _database.Add(member, _platform);
        }
    }
}
