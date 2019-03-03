using System.Threading.Tasks;
using BFF.Persistence.Realm.ORM.Interfaces;
using Realms;

namespace BFF.Persistence.Realm.Persistence.Models
{
    public interface IFlagRealm : IUniquelyNamedPersistenceModelRealm
    {
        long Color { get; set; }
    }
    
    internal class Flag : RealmObject, IFlagRealm
    {
        private readonly ICrudOrm<Flag> _crudOrm;

        public Flag()
        {
        }

        public Flag(bool isInserted, ICrudOrm<Flag> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public string Name { get; set; }
        public long Color { get; set; }

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
