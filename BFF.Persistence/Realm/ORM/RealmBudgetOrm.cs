using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmBudgetOrm : IBudgetOrm
    {
        private readonly IRealmOperations _realmOperations;

        private class OutflowResponse
        {
            public DateTime Month { get; set; }
            public long Sum { get; set; }
        }

        public RealmBudgetOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public async Task<BudgetBlock> FindAsync(int year, ICategoryRealm[] categories,
            (ICategoryRealm category, int MonthOffset)[] incomeCategories)
        {
            return new BudgetBlock()
            {
                BudgetEntriesPerMonth = new Dictionary<DateTime, IList<(IBudgetEntryRealm Entry, long Outflow, long Balance)>>(),
                IncomesPerMonth = new Dictionary<DateTime, long>(),
                DanglingTransfersPerMonth = new Dictionary<DateTime, long>(),
                UnassignedTransactionsPerMonth = new Dictionary<DateTime, long>()
            }; 
        }
    }

    public class BudgetBlock
    {
        public IDictionary<DateTime, IList<(IBudgetEntryRealm Entry, long Outflow, long Balance)>> BudgetEntriesPerMonth
        {
            get;
            set;
        }

        public long InitialNotBudgetedOrOverbudgeted { get; set; }

        public long InitialOverspentInPreviousMonth { get; set; }

        public IDictionary<DateTime, long> IncomesPerMonth { get; set; }

        public IDictionary<DateTime, long> DanglingTransfersPerMonth { get; set; }

        public IDictionary<DateTime, long> UnassignedTransactionsPerMonth { get; set; }
    }
}
