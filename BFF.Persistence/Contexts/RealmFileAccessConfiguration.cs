using BFF.Model.ImportExport;

namespace BFF.Persistence.Contexts
{
    internal class RealmFileAccessConfiguration : IRealmFileAccessConfiguration
    {
        public RealmFileAccessConfiguration(
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
