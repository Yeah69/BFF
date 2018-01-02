using System.Data.Common;
using System.Data.SQLite;

namespace BFF.DB.SQLite
{
    public interface IProvideSqLiteConnetion : IProvideConnection
    { }

    public class ProvideSqLiteConnection : IProvideSqLiteConnetion
    {
        public DbConnection Connection  => new SQLiteConnection(ConnectionString) ;
        
        private string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;Pooling=True;Max Pool Size=100;";

        private string DbPath { get; }

        public ProvideSqLiteConnection(string dbPath)
        {
            DbPath = dbPath;
        }
    }
}