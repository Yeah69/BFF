using System.Data.Common;
using System.Data.SQLite;

namespace BFF.DB.SQLite
{
    public class ProvideSqLiteConnection : IProvideConnection
    {
        private readonly string _connectionString;
        
        public DbConnection Connection  => new SQLiteConnection(_connectionString) ;

        public ProvideSqLiteConnection(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}