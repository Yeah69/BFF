using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal class AccountComparer : Comparer<IAccount>
    {
        public override int Compare(IAccount x, IAccount y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IAccountRepositoryInternal : IAccountRepository, IReadOnlyRepository<IAccount>
    {
    }

    internal sealed class AccountRepository : ObservableRepositoryBase<IAccount, IAccountSql>, IAccountRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IAccountSql> _crudOrm;
        private readonly Lazy<IAccountOrm> _accountOrm;
        private readonly Lazy<ITransRepository> _transRepository;

        public AccountRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IAccountSql> crudOrm,
            Lazy<IAccountOrm> accountOrm,
            Lazy<ITransRepository> transRepository) : base(rxSchedulerProvider, crudOrm, new AccountComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
        }

        protected override Task<IAccount> ConvertToDomainAsync(IAccountSql persistenceModel) =>
            Task.FromResult<IAccount>(
                new Models.Domain.Account(
                    _crudOrm,
                    _accountOrm.Value,
                    _transRepository.Value,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.StartingDate,
                    persistenceModel.Name,
                    persistenceModel.StartingBalance));
    }
}