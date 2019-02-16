using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    internal class AccountComparer : Comparer<IAccount>
    {
        public override int Compare(IAccount x, IAccount y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IAccountRepository : IObservableRepositoryBase<IAccount>
    {
        Task<long?> GetClearedBalanceAsync(IAccount account);

        Task<long?> GetClearedBalanceUntilNowAsync(IAccount account);

        Task<long?> GetUnclearedBalanceAsync(IAccount account);

        Task<long?> GetUnclearedBalanceUntilNowAsync(IAccount account);
    }

    internal interface IAccountRepositoryInternal : IAccountRepository, IReadOnlyRepository<IAccount>
    {
    }

    internal sealed class AccountRepository : ObservableRepositoryBase<IAccount, IAccountSql>, IAccountRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountOrm _accountOrm;
        private readonly Func<IAccountSql> _accountDtoFactory;

        public AccountRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm, 
            IAccountOrm accountOrm,
            Func<IAccountSql> accountDtoFactory) : base(provideConnection, rxSchedulerProvider, crudOrm, new AccountComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountOrm = accountOrm;
            _accountDtoFactory = accountDtoFactory;
        }

        protected override Converter<IAccount, IAccountSql> ConvertToPersistence => domainAccount =>
        {
            var accountDto = _accountDtoFactory();

            accountDto.Id = domainAccount.Id;
            accountDto.Name = domainAccount.Name;
            accountDto.StartingBalance = domainAccount.StartingBalance;
            accountDto.StartingDate = domainAccount.StartingDate;

            return accountDto;
        };

        protected override Task<IAccount> ConvertToDomainAsync(IAccountSql persistenceModel) =>
            Task.FromResult<IAccount>(
                new Account(
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.StartingDate,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.StartingBalance));

        public Task<long?> GetClearedBalanceAsync(IAccount account)
        {
            try
            {
                switch (account)
                {
                    case ISummaryAccount _:
                        return _accountOrm.GetClearedOverallBalanceAsync();
                    case IAccount specificAccount:
                        return _accountOrm.GetClearedBalanceAsync(specificAccount.Id);
                    default:
                        return Task.FromResult<long?>(null);
                }
            }
            catch (OverflowException)
            {
                return Task.FromResult<long?>(null);
            }
        }

        public Task<long?> GetClearedBalanceUntilNowAsync(IAccount account)
        {
            try
            {
                switch (account)
                {
                    case ISummaryAccount _:
                        return _accountOrm.GetClearedOverallBalanceUntilNowAsync();
                    case IAccount specificAccount:
                        return _accountOrm.GetClearedBalanceUntilNowAsync(specificAccount.Id);
                    default:
                        return Task.FromResult<long?>(null);
                }
            }
            catch (OverflowException)
            {
                return Task.FromResult<long?>(null);
            }
        }

        public Task<long?> GetUnclearedBalanceAsync(IAccount account)
        {
            try
            {
                switch (account)
                {
                    case ISummaryAccount _:
                        return _accountOrm.GetUnclearedOverallBalanceAsync();
                    case IAccount specificAccount:
                        return _accountOrm.GetUnclearedBalanceAsync(specificAccount.Id);
                    default:
                        return Task.FromResult<long?>(null);
                }
            }
            catch (OverflowException)
            {
                return Task.FromResult<long?>(null);
            }
        }

        public Task<long?> GetUnclearedBalanceUntilNowAsync(IAccount account)
        {
            try
            {
                switch (account)
                {
                    case ISummaryAccount _:
                        return _accountOrm.GetUnclearedOverallBalanceUntilNowAsync();
                    case IAccount specificAccount:
                        return _accountOrm.GetUnclearedBalanceUntilNowAsync(specificAccount.Id);
                    default:
                        return Task.FromResult<long?>(null);
                }
            }
            catch (OverflowException)
            {
                return Task.FromResult<long?>(null);
            }
        }
    }
}