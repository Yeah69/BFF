using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class DbSetting : Model.Models.DbSetting, IRealmModel<IDbSettingRealm>
    {
        private readonly RealmObjectWrap<IDbSettingRealm> _realmObjectWrap;

        public DbSetting(
            ICrudOrm<IDbSettingRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            IDbSettingRealm realmObject) : base(rxSchedulerProvider)
        {
            _realmObjectWrap = new RealmObjectWrap<IDbSettingRealm>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.DbSetting { Id = 0 };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(IDbSettingRealm ro)
            {
                ro.CurrencyCultureName = CurrencyCultureName;
                ro.DateCultureName = DateCultureName;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public IDbSettingRealm RealmObject => _realmObjectWrap.RealmObject;

        public override Task InsertAsync()
        {
            return _realmObjectWrap.InsertAsync();
        }

        public override Task DeleteAsync()
        {
            return _realmObjectWrap.DeleteAsync();
        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
        }
    }
}
