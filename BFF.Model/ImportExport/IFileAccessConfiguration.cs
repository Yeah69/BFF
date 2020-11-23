namespace BFF.Model.ImportExport
{
    public interface ICreateFileAccessConfiguration
    {
        IFileAccessConfiguration Create(string path);
        IFileAccessConfiguration CreateWithEncryption(string path, string password);
    }

    public interface IFileAccessConfiguration : ILoadProjectConfiguration
    {
        string Path { get; }
    }

    public interface IRealmFileAccessConfiguration : IFileAccessConfiguration
    {
        string? Password { get; }
    }

    public interface ISqliteFileAccessConfiguration : IFileAccessConfiguration
    {
    }
}