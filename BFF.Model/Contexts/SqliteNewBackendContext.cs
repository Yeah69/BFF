using System;
using BFF.Core.Persistence;
using BFF.Persistence.Contexts;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Contexts
{

    public interface ISqliteNewBackendContext : INewBackendContext
    {
    }

    internal class SqliteNewBackendContext : ISqliteNewBackendContext
    {
        private readonly Func<ICreateBackendOrm> _createBackendOrmFactory;

        public SqliteNewBackendContext(
            IPersistenceConfiguration persistenceConfiguration,
            Func<IPersistenceConfiguration, IPersistenceContext> persistenceContextFactory,
            Func<ICreateBackendOrm> createBackendOrmFactory)
        {
            _createBackendOrmFactory = createBackendOrmFactory;
            persistenceContextFactory(persistenceConfiguration);
        }

        public void CreateNewBackend()
        {
            _createBackendOrmFactory().CreateAsync();
        }
    }
}