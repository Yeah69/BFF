namespace BFF.Model.Contexts
{

    public interface ISqliteNewBackendContext : INewBackendContext
    {
    }

    internal class SqliteNewBackendContext : ISqliteNewBackendContext
    {
        private readonly ICreateBackend _createBackend;

        public SqliteNewBackendContext(
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