using System.Threading.Tasks;
using BFF.Persistence.ORM.Realm.Interfaces;
using Realms;

namespace BFF.Persistence.Models.Realm
{
    public interface IDbSettingRealm : IUniqueIdPersistenceModelRealm
    {
        string CurrencyCultureName { get; set; }
        string DateCultureName { get; set; }
    }
    
    internal class DbSetting : RealmObject, IDbSettingRealm
    {
        private readonly ICrudOrm<DbSetting> _crudOrm;

        public DbSetting(bool isInserted, ICrudOrm<DbSetting> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public int Id { get; set; } = 0;
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";

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