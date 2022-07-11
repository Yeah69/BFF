using System;
using System.IO;
using System.Linq;
using BFF.Core.Extensions;
using BFF.Model.ImportExport;
using BFF.Persistence.Contexts;

namespace BFF.Persistence.Common
{
    internal abstract class ProvideConnectionBase<T> : IProvideConnection<T>
    {
        private readonly IDateTimeStaticDelegate _dateTimeStaticDelegate;

        public abstract T Connection { get; }

        public void Backup(string reason)
        {
            var fileInfo = new FileInfo(DbPath);
            var directory = fileInfo.Directory;

            if(directory is null) throw new DirectoryNotFoundException();

            var backupDirectoryPath = BackupDirectoryPath(fileInfo.Name);
            var backupDirectory = directory.GetDirectories().FirstOrDefault(di => di.Name == backupDirectoryPath) ??
                                  directory.CreateSubdirectory(backupDirectoryPath);

            fileInfo.CopyTo($"{backupDirectory.FullName}{Path.DirectorySeparatorChar}{_dateTimeStaticDelegate.Now:yyyy-MM-dd_HH-mm-ss}_{reason.RemoveIllegalFilePathCharacters()}_{fileInfo.Name}");
        }
        protected string DbPath { get; }
        private static string BackupDirectoryPath(string fileName) => $"BFF_{fileName}_Backups";

        protected ProvideConnectionBase(
            IRealmProjectFileAccessConfiguration config,
            IDateTimeStaticDelegate dateTimeStaticDelegate)
        {
            _dateTimeStaticDelegate = dateTimeStaticDelegate;
            DbPath = config.Path;
        }
    }
}