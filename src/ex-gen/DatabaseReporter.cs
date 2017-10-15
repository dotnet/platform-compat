using Microsoft.Cci;
using PlatformCompat.Scanner;

namespace ex_gen
{
    internal sealed class DatabaseReporter : IExceptionReporter
    {
        private readonly Database[] _databases;
        private readonly string _platform;

        public DatabaseReporter(Database[] databases, string platform)
        {
            _databases = databases;
            _platform = platform;
        }

        public void Report(ExceptionInfo result, ITypeDefinitionMember member)
        {
            if (result.Throws)
            {
                foreach(var database in _databases)
                    database.Add(member, result.Site, _platform);
            }
        }
    }
}
