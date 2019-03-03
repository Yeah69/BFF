using System.Threading.Tasks;
using BFF.Persistence.Realm.ORM.Interfaces;
using Realms;

namespace BFF.Persistence.Realm.Persistence.Models
{
    public interface IPayeeRealm : IUniquelyNamedPersistenceModelRealm
    {
    }
    
    internal class Payee : RealmObject, IPayeeRealm
    {
        private readonly ICrudOrm<Payee> _crudOrm;

        public Payee()
        {
        }

        public Payee(bool isInserted, ICrudOrm<Payee> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public string Name { get; set; }

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