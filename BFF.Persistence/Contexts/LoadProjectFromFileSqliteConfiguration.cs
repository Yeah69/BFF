using BFF.Core.Persistence;

namespace BFF.Persistence.Contexts
{
    internal class SqliteFileAccessConfiguration : ISqliteFileAccessConfiguration
    {
        public SqliteFileAccessConfiguration(
            string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
