using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace BFF.DB.SQLite
{
    public interface IProvideSqLiteConnection : IProvideConnection
    { }

    public class ProvideSqLiteConnection : IProvideSqLiteConnection
    {
        public DbConnection Connection  => new SQLiteConnection(ConnectionString);

        public void Backup(string reason)
        {
            var fileInfo = new FileInfo(DbPath);
            var directory = fileInfo.Directory;

            if(directory == null) throw new DirectoryNotFoundException();

            var backupDirectoryPath = BackupDirectoryPath(fileInfo.Name);
            var backupDirectory = directory.GetDirectories().FirstOrDefault(di => di.Name == backupDirectoryPath) ??
                                  directory.CreateSubdirectory(backupDirectoryPath);

            fileInfo.CopyTo($"{backupDirectory.FullName}{Path.DirectorySeparatorChar}{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{reason}_{fileInfo.Name}");
        }

        private string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;Pooling=True;Max Pool Size=100;";

        private string DbPath { get; }

        public ProvideSqLiteConnection(string dbPath)
        {
            DbPath = dbPath;
        }

        private string BackupDirectoryPath(string fileName) => $"BFF_{fileName}_Backups";
    }
}