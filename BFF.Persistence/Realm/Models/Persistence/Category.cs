using System.Linq;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class Category : RealmObject, IUniqueIdPersistenceModelRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Category Parent { get; set; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int Month { get; set; }

        [Backlink(nameof(BudgetCacheEntry.Category))]
        public IQueryable<BudgetCacheEntry> BudgetCacheEntries { get; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Category other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}