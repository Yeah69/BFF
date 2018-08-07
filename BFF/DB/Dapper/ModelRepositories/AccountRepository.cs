using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class AccountComparer : Comparer<Domain.IAccount>
    {
        public override int Compare(Domain.IAccount x, Domain.IAccount y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IAccountRepository : IObservableRepositoryBase<Domain.IAccount>
    {
        Task<long?> GetClearedBalanceAsync(Domain.IAccount account);

        Task<long?> GetClearedBalanceUntilNowAsync(Domain.IAccount account);

        Task<long?> GetUnclearedBalanceAsync(Domain.IAccount account);

        Task<long?> GetUnclearedBalanceUntilNowAsync(Domain.IAccount account);
    }

    public sealed class AccountRepository : ObservableRepositoryBase<Domain.IAccount, Account>, IAccountRepository
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

        protected override Converter<Domain.IAccount, Account> ConvertToPersistence => domainAccount => 
            new Account
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance,
                StartingDate = domainAccount.StartingDate
            };

        protected override Task<Domain.IAccount> ConvertToDomainAsync(Account persistenceModel) =>
            Task.FromResult<Domain.IAccount>(
                new Domain.Account(
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.StartingDate,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.StartingBalance));

        public Task<long?> GetClearedBalanceAsync(Domain.IAccount account)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetClearedOverallBalanceAsync();
                    case Domain.IAccount specificAccount:
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

        public Task<long?> GetClearedBalanceUntilNowAsync(Domain.IAccount account)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetClearedOverallBalanceUntilNowAsync();
                    case Domain.IAccount specificAccount:
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

        public Task<long?> GetUnclearedBalanceAsync(Domain.IAccount account)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetUnclearedOverallBalanceAsync();
                    case Domain.IAccount specificAccount:
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

        public Task<long?> GetUnclearedBalanceUntilNowAsync(Domain.IAccount account)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetUnclearedOverallBalanceUntilNowAsync();
                    case Domain.IAccount specificAccount:
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