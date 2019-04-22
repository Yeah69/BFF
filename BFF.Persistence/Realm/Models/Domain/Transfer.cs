using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transfer : Model.Models.Transfer, IRealmModel<ITransRealm>
    {
        private readonly ICrudOrm<ITransRealm> _crudOrm;
        private bool _isInserted;

        public Transfer(
            ICrudOrm<ITransRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            ITransRealm realmObject,
            bool isInserted,
            DateTime date,
            IFlag flag, 
            string checkNumber, 
            IAccount fromAccount,
            IAccount toAccount,
            string memo, 
            long sum,
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, fromAccount, toAccount, memo, sum, cleared)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _isInserted = isInserted;
        }

        public ITransRealm RealmObject { get; }

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
