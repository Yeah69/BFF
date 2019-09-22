using System;
using BFF.Core.IoC;
using BFF.Core.Persistence;

namespace BFF.Persistence.Contexts
{
    internal class CreateFileAccessConfiguration : ICreateFileAccessConfiguration, IOncePerApplication
    {
        public IFileAccessConfiguration Create(string path)
        {
            if (path.EndsWith(".sqlite") || path.EndsWith(".bffs"))
                return new SqliteFileAccessConfiguration(path);
            if (path.EndsWith(".realm"))
                return new RealmFileAccessConfiguration(path, null);
            throw new ArgumentException("Cannot infer BFF file from given path", nameof(path));
        }

        public IFileAccessConfiguration CreateWithEncryption(string path, string password)
        {
            if (path.EndsWith(".realm"))
                return new RealmFileAccessConfiguration(path, password);
            throw new ArgumentException("Cannot infer BFF file from given path", nameof(path));
        }
    }
}
