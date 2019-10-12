using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class DbSetting : Model.Models.DbSetting, IRealmModel<Persistence.DbSetting>
    {
        private readonly RealmObjectWrap<Persistence.DbSetting> _realmObjectWrap;

        public DbSetting(
            ICrudOrm<Persistence.DbSetting> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            Persistence.DbSetting realmObject) : base(rxSchedulerProvider)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.DbSetting>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.DbSetting { Id = 0 };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(Persistence.DbSetting ro)
            {
                ro.CurrencyCultureName = CurrencyCultureName;
                ro.DateCultureName = DateCultureName;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.DbSetting RealmObject => _realmObjectWrap.RealmObject;

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
