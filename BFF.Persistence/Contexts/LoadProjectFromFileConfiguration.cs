using BFF.Core.Persistence;

namespace BFF.Persistence.Contexts
{
    internal class LoadProjectFromFileConfiguration : ILoadProjectFromFileConfiguration
    {
        public LoadProjectFromFileConfiguration(
            string path)
        {
            Path = path;
            if (path.EndsWith(".sqlite") || path.EndsWith(".bffs"))
                BackendChoice = Core.IoC.BackendChoice.Sqlite;
            else if (path.EndsWith(".realm") || path.EndsWith(".bffr"))
                BackendChoice = Core.IoC.BackendChoice.Realm;
            else
                BackendChoice = Core.IoC.BackendChoice.Unknown;
        }

        public string Path { get; }
        public Core.IoC.BackendChoice BackendChoice { get; }
    }
}
