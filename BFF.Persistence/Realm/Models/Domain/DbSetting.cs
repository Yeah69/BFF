using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class DbSetting : Model.Models.DbSetting, IRealmModel<IDbSettingRealm>
    {
        private readonly ICrudOrm<IDbSettingRealm> _crudOrm;

        public DbSetting(
            ICrudOrm<IDbSettingRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            IDbSettingRealm realmObject) : base(rxSchedulerProvider)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
        }

        public IDbSettingRealm RealmObject { get; }

        public override bool IsInserted => true;
        public override async Task InsertAsync()
        {
            await _crudOrm.CreateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(RealmObject).ConfigureAwait(false);
        }
    }
}
