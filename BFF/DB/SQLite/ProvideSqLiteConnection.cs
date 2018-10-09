using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using BFF.Helper.Extensions;
using Dapper;

namespace BFF.DB.SQLite
{
    public class NullableLongTypeHandler : SqlMapper.TypeHandler<long?>
    {
        public override void SetValue(IDbDataParameter parameter, long? value)
        {
            parameter.Value = value;
        }

        public override long? Parse(object value)
        {
            if (value == DBNull.Value || value == null)
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

    public interface IProvideSqLiteConnection : IProvideConnection
    { }

    public class ProvideSqLiteConnection : IProvideSqLiteConnection
    {
        static ProvideSqLiteConnection()
        {
            SqlMapper.AddTypeHandler(new NullableLongTypeHandler());
        }

        public IDbConnection Connection
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

        public void Backup(string reason)
        {
            var fileInfo = new FileInfo(DbPath);
            var directory = fileInfo.Directory;

            if(directory == null) throw new DirectoryNotFoundException();

            var backupDirectoryPath = BackupDirectoryPath(fileInfo.Name);
            var backupDirectory = directory.GetDirectories().FirstOrDefault(di => di.Name == backupDirectoryPath) ??
                                  directory.CreateSubdirectory(backupDirectoryPath);

            fileInfo.CopyTo($"{backupDirectory.FullName}{Path.DirectorySeparatorChar}{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{reason.RemoveIllegalFilePathCharacters()}_{fileInfo.Name}");
        }

        private string ConnectionString => $"Data Source={DbPath};";

        private string DbPath { get; }

        public ProvideSqLiteConnection(string dbPath)
        {
            DbPath = dbPath;
        }

        private string BackupDirectoryPath(string fileName) => $"BFF_{fileName}_Backups";
    }
}