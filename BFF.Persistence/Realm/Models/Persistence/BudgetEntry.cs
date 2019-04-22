using System;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface IBudgetEntryRealm : IUniqueIdPersistenceModelRealm, IHaveCategoryRealm
    {
        DateTime Month { get; set; }
        long Budget { get; set; }
    }
    
    internal class BudgetEntry : RealmObject, IBudgetEntryRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Category CategoryRef { get; set; }
        [Ignored]
        public ICategoryRealm Category { get => CategoryRef; set => CategoryRef = value as Category; }
        [Ignored]
        public DateTime Month { get => MonthOffset.UtcDateTime; set => MonthOffset = value; }
        public DateTimeOffset MonthOffset { get; set; }
        public long Budget { get; set; }
    }
}