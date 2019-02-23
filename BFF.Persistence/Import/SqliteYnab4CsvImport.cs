using System;
using BFF.Core.Helper;
using BFF.Core.Persistence;

namespace BFF.Persistence.Import
{
    internal class SqliteYnab4CsvImport : Ynab4CsvImportBase
    {
        private readonly Func<SqliteYnab4CsvImportContainer> _sqliteYnab4CsvImportContainerFactory;

        protected SqliteYnab4CsvImport(
            IYnab4ImportConfiguration configuration, 
            Func<SqliteYnab4CsvImportContainer> sqliteYnab4CsvImportContainerFactory,
            ILocalizer localizer) : base(configuration, localizer)
        {
            _sqliteYnab4CsvImportContainerFactory = sqliteYnab4CsvImportContainerFactory;
        }

        protected override IYnab4CsvImportContainer CreateContainer()
        {
            return _sqliteYnab4CsvImportContainerFactory();
        }
    }
}