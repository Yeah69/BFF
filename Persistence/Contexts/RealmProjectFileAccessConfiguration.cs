using BFF.Model.ImportExport;

namespace BFF.Persistence.Contexts
{
    internal class RealmProjectFileAccessConfiguration : IRealmProjectFileAccessConfiguration
    {
        public RealmProjectFileAccessConfiguration()
            //string path,
            //string? password)
        {
            Path = "C:\\test.realm"; // path;
            Password = "asdf";
        }

        public string Path { get; }
        public string? Password { get; }
    }
}
