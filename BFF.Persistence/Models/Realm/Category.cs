using System.Threading.Tasks;
using BFF.Persistence.ORM.Realm.Interfaces;
using Realms;

namespace BFF.Persistence.Models.Realm
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
        private readonly ICrudOrm<Category> _crudOrm;

        public Category(bool isInserted, ICrudOrm<Category> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public int Id { get; set; }
        public Category ParentRef { get; set; }
        [Ignored]
        public ICategoryRealm Parent { get => ParentRef; set => ParentRef = value as Category; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int MonthOffset { get; set; }

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