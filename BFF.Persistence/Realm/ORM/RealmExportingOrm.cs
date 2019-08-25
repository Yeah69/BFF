using System;
using System.Threading.Tasks;
using BFF.Persistence.Import;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmExportingOrm : IExportingOrm
    {
        private readonly IRealmOperations _realmOperations;
        private readonly Func<DbSetting> _dbSettingFactory;

        public RealmExportingOrm(
            IRealmOperations realmOperations, 
            Func<DbSetting> dbSettingFactory)
        {
            _realmOperations = realmOperations;
            _dbSettingFactory = dbSettingFactory;
        }

        public Task PopulateDatabaseAsync(IRealmExportContainerData sqliteExportContainer)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {

                realm.Write(() =>
                {
                    sqliteExportContainer.Accounts.ForEach(a => realm.Add(a as RealmObject, update: false));
                    sqliteExportContainer.Payees.ForEach(p => realm.Add(p as RealmObject, update: false));
                    sqliteExportContainer.RootCategories.ForEach(mc => realm.Add(mc as RealmObject, update: false));
                    sqliteExportContainer.IncomeCategories.ForEach(ic => realm.Add(ic as RealmObject, update: false));
                    sqliteExportContainer.Flags.ForEach(f => realm.Add(f as RealmObject, update: false));
                    sqliteExportContainer.Trans.ForEach(t => realm.Add(t as RealmObject, update: false));
                    sqliteExportContainer.SubTransactions.ForEach(st => realm.Add(st as RealmObject, update: false));
                    sqliteExportContainer.BudgetEntries.ForEach(be => realm.Add(be as RealmObject, update: false));
                    realm.Add(_dbSettingFactory());
                });
            }
        }
    }
}
