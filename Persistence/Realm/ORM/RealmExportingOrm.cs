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
                    exportContainer.Accounts.ForEach(a => realm.Add(a as RealmObject));
                    exportContainer.Payees.ForEach(p => realm.Add(p as RealmObject));
                    exportContainer.Categories.ForEach(mc => realm.Add(mc as RealmObject));
                    exportContainer.IncomeCategories.ForEach(ic => realm.Add(ic as RealmObject));
                    exportContainer.Flags.ForEach(f => realm.Add(f as RealmObject));
                    exportContainer.Trans.ForEach(t => realm.Add(t as RealmObject));
                    exportContainer.SubTransactions.ForEach(st => realm.Add(st as RealmObject));
                    exportContainer.BudgetEntries.ForEach(be => realm.Add(be as RealmObject));
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
