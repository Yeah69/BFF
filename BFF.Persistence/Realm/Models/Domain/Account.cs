using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Account : Model.Models.Account, IRealmModel<Persistence.Account>
    {
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly IAccountOrm _accountOrm;
        private readonly IRealmAccountRepositoryInternal _repository;
        private readonly IRealmTransRepository _transRepository;
        private readonly RealmObjectWrap<Persistence.Account> _realmObjectWrap;

        public Account(
            ICrudOrm<Persistence.Account> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IAccountOrm accountOrm,
            IRealmAccountRepositoryInternal repository,
            IRealmTransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            Persistence.Account realmObject,
            DateTime startingDate, 
            string name, 
            long startingBalance) 
            : base(
                rxSchedulerProvider, 
                startingDate,
                name, 
                startingBalance)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.Account>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.Account();
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _updateBudgetCache = updateBudgetCache;
            _accountOrm = accountOrm;
            _repository = repository;
            _transRepository = transRepository;
            
            void UpdateRealmObject(Persistence.Account ro)
            {
                ro.Name = Name;
                ro.StartingBalance = StartingBalance;
                ro.StartingDate = new DateTimeOffset(StartingDate, TimeSpan.Zero);
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.Account RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _repository.AddAsync(this).ConfigureAwait(false);
            await _updateBudgetCache.OnAccountInsertOrDelete(new DateTimeOffset(StartingDate, TimeSpan.Zero)).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            var date = new DateTimeOffset(StartingDate, TimeSpan.Zero);
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
            await _updateBudgetCache.OnAccountInsertOrDelete(date)
                .ConfigureAwait(false);
        }

        protected override async Task UpdateAsync()
        {
            var beforeStartingDate = _realmObjectWrap.RealmObject?.StartingDate;
            var beforeStartingBalance = _realmObjectWrap.RealmObject?.StartingBalance;
            // The change already occured for the domain model but wasn't yet updated in the realm object
            var afterStartingDate = new DateTimeOffset(StartingDate, TimeSpan.Zero);
            var afterStartingBalance = StartingBalance;
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            if (beforeStartingDate is null) return;
            await _updateBudgetCache
                .OnAccountChange(beforeStartingDate.Value, beforeStartingBalance.Value, afterStartingDate, afterStartingBalance)
                .ConfigureAwait(false);
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
