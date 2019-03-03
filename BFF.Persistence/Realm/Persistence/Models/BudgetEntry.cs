using System;
using System.Threading.Tasks;
using BFF.Persistence.Realm.ORM.Interfaces;
using Realms;

namespace BFF.Persistence.Realm.Persistence.Models
{
    public interface IBudgetEntryRealm : IUniqueIdPersistenceModelRealm, IHaveCategoryRealm
    {
        DateTimeOffset Month { get; set; }
        long Budget { get; set; }
    }
    
    internal class BudgetEntry : RealmObject, IBudgetEntryRealm
    {
        private readonly ICrudOrm<BudgetEntry> _crudOrm;

        public BudgetEntry()
        {
        }

        public BudgetEntry(bool isInserted, ICrudOrm<BudgetEntry> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public int Id { get; set; }
        public Category CategoryRef { get; set; }
        [Ignored]
        public ICategoryRealm Category { get => CategoryRef; set => CategoryRef = value as Category; }
        public DateTimeOffset Month { get; set; }
        public long Budget { get; set; }

        public Task<bool> InsertAsync()
        {
            return _crudOrm.CreateAsync(this);
        }

        public Task UpdateAsync()
        {
            return _crudOrm.UpdateAsync(this);
        }

        public Task DeleteAsync()
        {
            return _crudOrm.DeleteAsync(this);
        }

        [Ignored]
        public bool IsInserted { get; set; }
    }
}