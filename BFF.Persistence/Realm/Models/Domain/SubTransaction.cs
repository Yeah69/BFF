using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class SubTransaction : Model.Models.SubTransaction, IRealmModel<ISubTransactionRealm>
    {
        private readonly ICrudOrm<ISubTransactionRealm> _crudOrm;
        private bool _isInserted;

        public SubTransaction(
            ICrudOrm<ISubTransactionRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            ISubTransactionRealm realmObject,
            bool isInserted,
            ICategoryBase category,
            string memo, 
            long sum) : base(rxSchedulerProvider, category, memo, sum)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _isInserted = isInserted;
        }

        public ISubTransactionRealm RealmObject { get; }

        public override bool IsInserted => _isInserted;

        public override async Task InsertAsync()
        {
            _isInserted = await _crudOrm.CreateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
            _isInserted = false;
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(RealmObject).ConfigureAwait(false);
        }
    }
}
