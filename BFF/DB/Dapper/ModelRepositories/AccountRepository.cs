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
        Task<long?> GetBalanceAsync(Domain.IAccount account);

        Task<long?> GetBalanceUntilNowAsync(Domain.IAccount account);
    }

    public sealed class AccountRepository : ObservableRepositoryBase<Domain.IAccount, Account>, IAccountRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountOrm _accountOrm;

        public AccountRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm, 
            IAccountOrm accountOrm) : base(provideConnection, crudOrm, new AccountComparer())
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

        public Task<long?> GetBalanceAsync(Domain.IAccount account)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetOverallBalanceAsync();
                    case Domain.IAccount specificAccount:
                        return _accountOrm.GetBalanceAsync(specificAccount.Id);
                    default:
                        return Task.FromResult<long?>(null);
                }
            }
            catch (OverflowException)
            {
                return Task.FromResult<long?>(null);
            }
        }

        public Task<long?> GetBalanceUntilNowAsync(Domain.IAccount account)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetOverallBalanceUntilNowAsync();
                    case Domain.IAccount specificAccount:
                        return _accountOrm.GetBalanceUntilNowAsync(specificAccount.Id);
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