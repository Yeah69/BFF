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

        public RealmExportingOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task PopulateDatabaseAsync(IRealmExportContainerData exportContainer)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {

                realm.Write(() =>
                {
                    exportContainer.Accounts.ForEach(a => realm.Add(a as RealmObject, update: false));
                    exportContainer.Payees.ForEach(p => realm.Add(p as RealmObject, update: false));
                    exportContainer.Categories.ForEach(mc => realm.Add(mc as RealmObject, update: false));
                    exportContainer.IncomeCategories.ForEach(ic => realm.Add(ic as RealmObject, update: false));
                    exportContainer.Flags.ForEach(f => realm.Add(f as RealmObject, update: false));
                    exportContainer.Trans.ForEach(t => realm.Add(t as RealmObject, update: false));
                    exportContainer.SubTransactions.ForEach(st => realm.Add(st as RealmObject, update: false));
                    exportContainer.BudgetEntries.ForEach(be => realm.Add(be as RealmObject, update: false));
                    var dbSetting = new DbSetting
                    {
                        NextSubTransactionId = exportContainer.SubTransactions.Count,
                        NextTransId = exportContainer.Trans.Count,
                        NextCategoryId = exportContainer.IncomeCategories.Count + exportContainer.Categories.Count,
                        NextBudgetEntryId = exportContainer.BudgetEntries.Count
                    };
                    realm.Add(dbSetting);
                });
            }
        }
    }
}
