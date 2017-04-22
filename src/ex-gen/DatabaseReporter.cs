using Microsoft.Cci;
using Terrajobst.PlatformCompat.Scanner;

namespace ex_gen
{
    internal sealed class DatabaseReporter : IExceptionReporter
    {
        private readonly Database _database;
        private readonly string _platform;

        public DatabaseReporter(Database database, string platform)
        {
            _database = database;
            _platform = platform;
        }

        public void Report(ExceptionInfo result, ITypeDefinitionMember member)
        {
            if (result.Throws)
                _database.Add(member, _platform, result.Site);
        }
    }
}
