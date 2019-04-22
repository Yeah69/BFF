using System;
using System.Threading.Tasks;
using BFF.Persistence.Import;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmImportingOrm : IImportingOrm
    {
        private readonly IProvideRealmConnection _provideConnection;
        private readonly Func<DbSetting> _dbSettingFactory;

        public RealmImportingOrm(IProvideRealmConnection provideConnection, Func<DbSetting> dbSettingFactory)
        {
            _provideConnection = provideConnection;
            _dbSettingFactory = dbSettingFactory;
        }

        public async Task PopulateDatabaseAsync(IRealmYnab4CsvImportContainerData sqliteYnab4CsvImportContainer)
        {
            var realm = _provideConnection.Connection;

            await realm.WriteAsync(r =>
            {
                sqliteYnab4CsvImportContainer.Accounts.ForEach(a => r.Add(a as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.Payees.ForEach(p => r.Add(p as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.MasterCategories.ForEach(mc => r.Add(mc as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.SubCategories.ForEach(sc => r.Add(sc as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.IncomeCategories.ForEach(ic => r.Add(ic as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.Flags.ForEach(f => r.Add(f as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.Trans.ForEach(t => r.Add(t as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.SubTransactions.ForEach(st => r.Add(st as RealmObject, update: false));
                sqliteYnab4CsvImportContainer.BudgetEntries.ForEach(be => r.Add(be as RealmObject, update: false));
                r.Add(_dbSettingFactory());
            }).ConfigureAwait(false);
        }
    }
}
