using System.Data.Common;
using System.Data.SQLite;

namespace BFF.DB.SQLite
{
    public class ProvideSqLiteConnection : IProvideConnection
    {
        public DbConnection Connection  => new SQLiteConnection(ConnectionString) ;
        
        private string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        private string DbPath { get; }

        public ProvideSqLiteConnection(string dbPath)
        {
            DbPath = dbPath;
        }
    }
}