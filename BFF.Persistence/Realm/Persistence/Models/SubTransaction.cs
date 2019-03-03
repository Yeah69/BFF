using System.Threading.Tasks;
using BFF.Persistence.Realm.ORM.Interfaces;
using Realms;

namespace BFF.Persistence.Realm.Persistence.Models
{
    public interface ISubTransactionRealm : IUniqueIdPersistenceModelRealm, IHaveCategoryRealm
    {
        ITransRealm Parent { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
    }
    
    internal class SubTransaction : RealmObject, ISubTransactionRealm
    {
        private readonly ICrudOrm<SubTransaction> _crudOrm;

        public SubTransaction()
        {
        }

        public SubTransaction(bool isInserted, ICrudOrm<SubTransaction> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public int Id { get; set; }
        public Trans ParentRef { get; set; }
        [Ignored]
        public ITransRealm Parent { get => ParentRef; set => ParentRef = value as Trans; }
        public Category CategoryRef { get; set; }
        [Ignored]
        public ICategoryRealm Category { get => CategoryRef; set => CategoryRef = value as Category; }
        public string Memo { get; set; }
        public long Sum { get; set; }

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