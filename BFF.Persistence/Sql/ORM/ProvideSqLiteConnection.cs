using System;
using System.Data;
using BFF.Model.ImportExport;
using BFF.Persistence.Common;
using BFF.Persistence.Contexts;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BFF.Persistence.Sql.ORM
{
    internal class NullableLongTypeHandler : SqlMapper.TypeHandler<long?>
    {
        public override void SetValue(IDbDataParameter parameter, long? value)
        {
            parameter.Value = value;
        }

        public override long? Parse(object value)
        {
            if (value == DBNull.Value || value is null)
                return null;

            switch (value)
            {
                case int intValue:
                    return Convert.ToInt64(intValue);
                case long longValue:
                    return longValue;
                case double doubleValue:
                    return Convert.ToInt64(doubleValue);
                default:
                    throw new DataException();
            }
        }
    }

    public interface IProvideSqliteConnection : IProvideConnection<IDbConnection>
    { }

    internal class ProvideSqliteConnection : ProvideConnectionBase<IDbConnection>, IProvideSqliteConnection
    {
        static ProvideSqliteConnection()
        {
            SqlMapper.AddTypeHandler(new NullableLongTypeHandler());
        }

        public override IDbConnection Connection
        {
            get
            {
                var sqLiteConnection = new SqliteConnection(ConnectionString);
                sqLiteConnection.Open();
                var command = sqLiteConnection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys=ON;";
                command.ExecuteNonQuery();
                return sqLiteConnection;
            }
        }

        protected override string ConnectionString => $"Data Source={DbPath};";

        public ProvideSqliteConnection(IFileAccessConfiguration config) : base(config)
        {
        }
    }
}