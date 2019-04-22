using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal class AccountComparer : Comparer<IAccount>
    {
        public override int Compare(IAccount x, IAccount y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IRealmAccountRepositoryInternal : IAccountRepository, IRealmReadOnlyRepository<IAccount, IAccountRealm>
    {
    }

    internal sealed class RealmAccountRepository : RealmObservableRepositoryBase<IAccount, IAccountRealm>, IRealmAccountRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IAccountRealm> _crudOrm;
        private readonly Lazy<IAccountOrm> _accountOrm;
        private readonly Lazy<IRealmTransRepository> _transRepository;

        public RealmAccountRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IAccountRealm> crudOrm,
            Lazy<IAccountOrm> accountOrm,
            Lazy<IRealmTransRepository> transRepository) : base(rxSchedulerProvider, crudOrm, new AccountComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
        }

        protected override Task<IAccount> ConvertToDomainAsync(IAccountRealm persistenceModel) =>
            Task.FromResult<IAccount>(
                new Models.Domain.Account(
                    _crudOrm,
                    _accountOrm.Value,
                    _transRepository.Value,
                    _rxSchedulerProvider,
                    persistenceModel,
                    true,
                    persistenceModel.StartingDate,
                    persistenceModel.Name,
                    persistenceModel.StartingBalance));
    }
}