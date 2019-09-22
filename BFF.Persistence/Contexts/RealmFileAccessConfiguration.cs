using BFF.Core.Persistence;
using JetBrains.Annotations;

namespace BFF.Persistence.Contexts
{
    internal class RealmFileAccessConfiguration : IRealmFileAccessConfiguration
    {
        public RealmFileAccessConfiguration(
            string path,
            [CanBeNull] string password)
        {
            Path = path;
            Password = password;
        }

        public string Path { get; }
        public string Password { get; }
    }
}
