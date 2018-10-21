using System;
using System.Collections.Generic;
using System.Text;

namespace BFF.Core.Persistence
{
    public interface ISqlitePersistenceConfiguration : IPersistenceConfiguration
    {
        string Path { get; }
    }

    internal class SqlitePersistenceConfiguration : ISqlitePersistenceConfiguration
    {
        public SqlitePersistenceConfiguration(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
