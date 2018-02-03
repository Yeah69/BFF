using System;
using System.Collections.Generic;
using System.Data.Common;
using BFF.DB.PersistenceModels;
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
        long? GetBalance(Domain.IAccount account, DbConnection connection = null);

        long? GetBalanceUntilNow(Domain.IAccount account, DbConnection connection = null);
    }

    public sealed class AccountRepository : ObservableRepositoryBase<Domain.IAccount, Account>, IAccountRepository
    {
        private readonly IAccountOrm _accountOrm;

        public AccountRepository(IProvideConnection provideConnection, ICrudOrm crudOrm, IAccountOrm accountOrm) : base(provideConnection, crudOrm, new AccountComparer())
        {
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

        protected override Converter<(Account, DbConnection), Domain.IAccount> ConvertToDomain => tuple =>
        {
            (Account persistenceAccount, _) = tuple;
            return new Domain.Account(this,
                persistenceAccount.StartingDate,
                persistenceAccount.Id,
                persistenceAccount.Name,
                persistenceAccount.StartingBalance);
        };

        public long? GetBalance(Domain.IAccount account, DbConnection connection = null)
        {
            try
            {
                switch(account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetOverallBalance();
                    case Domain.IAccount specificAccount:
                        return _accountOrm.GetBalance(specificAccount.Id);
                    default:
                        return null;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        public long? GetBalanceUntilNow(Domain.IAccount account, DbConnection connection = null)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return _accountOrm.GetOverallBalanceUntilNow();
                    case Domain.IAccount specificAccount:
                        return _accountOrm.GetBalanceUntilNow(specificAccount.Id);
                    default:
                        return null;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }
    }
}