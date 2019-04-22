using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Account : Model.Models.Account, IRealmModel<IAccountRealm>
    {
        private readonly ICrudOrm<IAccountRealm> _crudOrm;
        private readonly IAccountOrm _accountOrm;
        private readonly IRealmTransRepository _transRepository;

        private bool _isInserted;

        public Account(
            ICrudOrm<IAccountRealm> crudOrm,
            IAccountOrm accountOrm,
            IRealmTransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            IAccountRealm realmObject,
            bool isInserted,
            DateTime startingDate, 
            string name, 
            long startingBalance) : base(rxSchedulerProvider, startingDate, name, startingBalance)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _accountOrm = accountOrm;
            _isInserted = isInserted;
            _transRepository = transRepository;
        }

        public long Id { get; private set; }

        public override bool IsInserted => _isInserted;

        public IAccountRealm RealmObject { get; }

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

        public override Task<long?> GetClearedBalanceAsync()
        {
            return _accountOrm.GetClearedBalanceAsync(RealmObject);
        }

        public override Task<long?> GetClearedBalanceUntilNowAsync()
        {
            return _accountOrm.GetClearedBalanceUntilNowAsync(RealmObject);
        }

        public override Task<long?> GetUnclearedBalanceAsync()
        {
            return _accountOrm.GetUnclearedBalanceAsync(RealmObject);
        }

        public override Task<long?> GetUnclearedBalanceUntilNowAsync()
        {
            return _accountOrm.GetUnclearedBalanceUntilNowAsync(RealmObject);
        }

        public override Task<IEnumerable<ITransBase>> GetTransPageAsync(int offset, int pageSize)
        {
            return _transRepository.GetPageAsync(offset, pageSize, this);
        }

        public override Task<long> GetTransCountAsync()
        {
            return _transRepository.GetCountAsync(this);
        }
    }
}
