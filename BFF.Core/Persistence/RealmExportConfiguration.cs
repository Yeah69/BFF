namespace BFF.Core.Persistence
{
    public interface IRealmExportConfiguration : IExportingConfiguration
    {
        string Path { get; }

        string Password { get; }
    }

    internal class RealmExportConfiguration : IRealmExportConfiguration
    {
        public RealmExportConfiguration(
            (string Path, string Password) data)
        {
            Path = data.Path;
            Password = data.Password;
        }
        public string Path { get; }
        public string Password { get; }
    }
}
