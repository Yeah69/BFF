using BFF.Model.ImportExport;

namespace BFF.Persistence.Contexts
{
    internal class RealmProjectFileAccessConfiguration : IRealmProjectFileAccessConfiguration
    {
        public RealmProjectFileAccessConfiguration(
            string path,
            string? password)
        {
            Path = path;
            Password = password;
        }

        public string Path { get; }
        public string? Password { get; }
    }
}
