namespace BFF.Model.ImportExport
{
    public interface ICreateFileAccessConfiguration
    {
        IRealmProjectFileAccessConfiguration Create(string path);
        IRealmProjectFileAccessConfiguration CreateWithEncryption(string path, string password);
    }

    public interface IRealmProjectFileAccessConfiguration : ILoadConfiguration, IExportConfiguration
    {
        string Path { get; }
        string? Password { get; }
    }
}