using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
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

        public AccountRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IAccountSql> crudOrm, 
            IAccountOrm accountOrm) : base(rxSchedulerProvider, crudOrm, new AccountComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountOrm = accountOrm;
        }

        protected override Task<IAccount> ConvertToDomainAsync(IAccountSql persistenceModel) =>
            Task.FromResult<IAccount>(
                new Account<IAccountSql>(
                    persistenceModel,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.StartingDate,
                    persistenceModel.Id > 0,
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
                        var dataModelInternal = specificAccount as IDataModelInternal<IAccountSql>;
                        if (dataModelInternal is null) return null;
                        return _accountOrm.GetClearedBalanceAsync(dataModelInternal.BackingPersistenceModel.Id);
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
                        var dataModelInternal = specificAccount as IDataModelInternal<IAccountSql>;
                        if (dataModelInternal is null) return null;
                        return _accountOrm.GetClearedBalanceUntilNowAsync(dataModelInternal.BackingPersistenceModel.Id);
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
                        var dataModelInternal = specificAccount as IDataModelInternal<IAccountSql>;
                        if (dataModelInternal is null) return null;
                        return _accountOrm.GetUnclearedBalanceAsync(dataModelInternal.BackingPersistenceModel.Id);
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
                        var dataModelInternal = specificAccount as IDataModelInternal<IAccountSql>;
                        if (dataModelInternal is null) return null;
                        return _accountOrm.GetUnclearedBalanceUntilNowAsync(dataModelInternal.BackingPersistenceModel.Id);
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