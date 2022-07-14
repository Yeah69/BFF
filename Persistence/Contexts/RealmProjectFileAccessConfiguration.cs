using BFF.Model.ImportExport;

namespace BFF.Persistence.Contexts
{
    internal class RealmProjectFileAccessConfiguration : IRealmProjectFileAccessConfiguration
    {
        public RealmProjectFileAccessConfiguration((string Path, string? Password) tuple)
        {
            Path = tuple.Path;
            Password = tuple.Password;
        }

        public string Path { get; }
        public string? Password { get; }
    }
}
