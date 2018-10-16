using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class AccountComparer : Comparer<IAccount>
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

    public sealed class AccountRepository : ObservableRepositoryBase<IAccount, AccountDto>, IAccountRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountOrm _accountOrm;

        public AccountRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm, 
            IAccountOrm accountOrm) : base(provideConnection, rxSchedulerProvider, crudOrm, new AccountComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountOrm = accountOrm;
        }

        protected override Converter<IAccount, AccountDto> ConvertToPersistence => domainAccount => 
            new AccountDto
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance,
                StartingDate = domainAccount.StartingDate
            };

        protected override Task<IAccount> ConvertToDomainAsync(AccountDto persistenceModel) =>
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