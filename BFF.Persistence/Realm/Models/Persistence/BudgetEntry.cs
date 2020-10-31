using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class BudgetEntry : RealmObject, IUniqueIdPersistenceModelRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Category? Category { get; set; }
        public int MonthIndex { get; set; }
        public long Budget { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is BudgetEntry other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}