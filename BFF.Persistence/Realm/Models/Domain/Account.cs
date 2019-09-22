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
        private readonly IAccountOrm _accountOrm;
        private readonly IRealmAccountRepositoryInternal _accountRepository;
        private readonly IRealmTransRepository _transRepository;
        private readonly RealmObjectWrap<IAccountRealm> _realmObjectWrap;

        public Account(
            ICrudOrm<IAccountRealm> crudOrm,
            IAccountOrm accountOrm,
            IRealmAccountRepositoryInternal accountRepository,
            IRealmTransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            IAccountRealm realmObject,
            DateTime startingDate, 
            string name, 
            long startingBalance) 
            : base(
                rxSchedulerProvider, 
                startingDate,
                name, 
                startingBalance)
        {
            _realmObjectWrap = new RealmObjectWrap<IAccountRealm>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.Account();
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _accountOrm = accountOrm;
            _accountRepository = accountRepository;
            _transRepository = transRepository;
            
            void UpdateRealmObject(IAccountRealm ro)
            {
                ro.Name = Name;
                ro.StartingBalance = StartingBalance;
                ro.StartingDate = StartingDate;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public IAccountRealm RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _accountRepository.AddAsync(this).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            _accountRepository.RemoveFromObservableCollection(this);
            _accountRepository.RemoveFromCache(this);
        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
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
