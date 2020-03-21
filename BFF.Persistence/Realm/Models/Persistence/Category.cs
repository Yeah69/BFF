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
        public int IncomeMonthOffset { get; set; }

        [Backlink(nameof(Trans.Category))]
        public IQueryable<Trans> Transactions { get; }

        [Backlink(nameof(SubTransaction.Category))]
        public IQueryable<SubTransaction> SubTransactions { get; }

        [Backlink(nameof(Parent))]
        public IQueryable<Category> Categories { get; }

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