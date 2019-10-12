using System;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class BudgetCacheEntry : RealmObject
    {
        public Category Category { get; set; }

        public DateTimeOffset Month { get; set; }

        public long Balance { get; set; }

        public long TotalBudget { get; set; }

        public long TotalNegativeBalance { get; set; }
    }
}
