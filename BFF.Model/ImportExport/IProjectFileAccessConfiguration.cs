namespace BFF.Model.ImportExport
{
    public interface ICreateFileAccessConfiguration
    {
        IProjectFileAccessConfiguration Create(string path);
        IProjectFileAccessConfiguration CreateWithEncryption(string path, string password);
    }

    public interface IProjectFileAccessConfiguration : ILoadConfiguration
    {
        string Path { get; }
    }

    public interface IEncryptedProjectFileAccessConfiguration : IProjectFileAccessConfiguration
    {
        string? Password { get; }
    }

    public interface IRealmProjectFileAccessConfiguration : IEncryptedProjectFileAccessConfiguration, IExportConfiguration
    {
    }

    public interface ISqliteProjectFileAccessConfiguration : IProjectFileAccessConfiguration, IImportConfiguration
    {
    }
}