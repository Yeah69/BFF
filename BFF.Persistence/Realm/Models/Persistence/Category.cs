using System.Linq;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface ICategoryRealm : IUniqueIdPersistenceModelRealm
    {
        ICategoryRealm Parent { get; set; }
        string Name { get; set; }
        bool IsIncomeRelevant { get; set; }
        int MonthOffset { get; set; }
    }
    
    internal class Category : RealmObject, ICategoryRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Category ParentRef { get; set; }
        [Ignored]
        public ICategoryRealm Parent { get => ParentRef; set => ParentRef = value as Category; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int MonthOffset { get; set; }

        [Backlink(nameof(BudgetCacheEntry.Category))]
        public IQueryable<BudgetCacheEntry> BudgetCacheEntriesRef { get; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is ICategoryRealm other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}