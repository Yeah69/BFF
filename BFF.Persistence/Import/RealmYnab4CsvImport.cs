using System;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Persistence.Contexts;

namespace BFF.Persistence.Import
{
    internal class RealmYnab4CsvImport : Ynab4CsvImportBase
    {
        private readonly Func<RealmYnab4CsvImportContainer> _realmYnab4CsvImportContainerFactory;

        protected RealmYnab4CsvImport(
            IYnab4ImportConfiguration configuration, 
            Func<RealmYnab4CsvImportContainer> sqliteYnab4CsvImportContainerFactory,
            ILocalizer localizer) : base(configuration, localizer)
        {
            _realmYnab4CsvImportContainerFactory = sqliteYnab4CsvImportContainerFactory;
        }

        protected override IYnab4CsvImportContainer CreateContainer()
        {
            return _realmYnab4CsvImportContainerFactory();
        }
    }
}