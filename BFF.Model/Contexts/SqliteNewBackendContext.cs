using BFF.Core.Persistence;

namespace BFF.Model.Contexts
{

    public interface ISqliteNewBackendContext : INewBackendContext
    {
    }

    internal class SqliteNewBackendContext : ISqliteNewBackendContext
    {
        private readonly ICreateBackend _createBackend;

        public SqliteNewBackendContext(
            IPersistenceConfiguration persistenceConfiguration,
            ICreateBackend createBackend)
        {
            _createBackend = createBackend;
        }

        public void CreateNewBackend()
        {
            _createBackend.CreateBackendBasedOnCurrentConfiguration();
        }
    }
}