using BFF.Model.ImportExport;

namespace BFF.Persistence.Contexts
{
    internal class SqliteProjectFileAccessConfiguration : ISqliteProjectFileAccessConfiguration
    {
        public SqliteProjectFileAccessConfiguration(
            string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
