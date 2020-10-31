using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal class AccountComparer : Comparer<IAccount>
    {
        public override int Compare(IAccount x, IAccount y)
        {
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }
    }

    internal interface IRealmAccountRepositoryInternal : IAccountRepository, IRealmObservableRepositoryBaseInternal<IAccount, Models.Persistence.Account>
    {
    }

    internal sealed class RealmAccountRepository : RealmObservableRepositoryBase<IAccount, Models.Persistence.Account>, IRealmAccountRepositoryInternal
    {
        private readonly ICrudOrm<Models.Persistence.Account> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IAccountOrm> _accountOrm;
        private readonly Lazy<IRealmTransRepository> _transRepository;

        public RealmAccountRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Models.Persistence.Account> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IAccountOrm> accountOrm,
            Lazy<IRealmTransRepository> transRepository) : base(rxSchedulerProvider, crudOrm, realmOperations, new AccountComparer())
        {
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
        }

        protected override Task<IAccount> ConvertToDomainAsync(Models.Persistence.Account persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            IAccount InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Account(
                    persistenceModel,
                    persistenceModel.StartingDate.UtcDateTime,
                    persistenceModel.Name,
                    persistenceModel.StartingBalance,
                    _crudOrm,
                    _accountOrm.Value,
                    this,
                    _transRepository.Value);
            }
        }
    }
}