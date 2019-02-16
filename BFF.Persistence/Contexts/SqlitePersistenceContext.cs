using System;
using BFF.Core.Persistence;
using BFF.Persistence.ORM.Sqlite;

namespace BFF.Persistence.Contexts
{
    public interface ISqlitePersistenceContext : IPersistenceContext
    {

    }

    internal class SqlitePersistenceContext : ISqlitePersistenceContext
    {
        public SqlitePersistenceContext(ISqlitePersistenceConfiguration configuration,
            Func<string, IProvideSqliteConnection> connectionProviderFactory)
        {
            connectionProviderFactory(configuration.Path);
        }
    }
}
