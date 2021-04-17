using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class DbSetting : RealmObject, IUniqueIdPersistenceModelRealm
    {
        [PrimaryKey]
        public int Id { get; set; } = 0;
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";
        public int NextCategoryId { get; set; }
        public int NextTransId { get; set; }
        public int NextSubTransactionId { get; set; }
        public int NextBudgetEntryId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is DbSetting other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}