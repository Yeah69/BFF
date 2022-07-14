using System;
using BFF.Core.IoC;
using BFF.Model.ImportExport;

namespace BFF.Persistence.Contexts
{
    internal class CreateFileAccessConfiguration : ICreateFileAccessConfiguration, IContainerInstance
    {
        public IRealmProjectFileAccessConfiguration Create(string path)
        {
            if (path.EndsWith(".realm"))
                return new RealmProjectFileAccessConfiguration((path, null));
            throw new ArgumentException("Cannot infer BFF file from given path", nameof(path));
        }

        public IRealmProjectFileAccessConfiguration CreateWithEncryption(string path, string password)
        {
            if (path.EndsWith(".realm"))
                return new RealmProjectFileAccessConfiguration((path, password));
            throw new ArgumentException("Cannot infer BFF file from given path", nameof(path));
        }
    }
}
