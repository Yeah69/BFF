using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IBffOrmAsync : ICrudOrmAsync, IPagedOrmAsync
    {
        string DbPath { get; }
        Task CreateNewDatabaseAsync(string dbPath);
        Task PopulateDatabaseAsync(IEnumerable<ITransaction> transactions, IEnumerable<ISubTransaction> subTransactions, IEnumerable<IIncome> incomes, IEnumerable<ISubIncome> subIncomes,
            IEnumerable<ITransfer> transfers, IEnumerable<IAccount> accounts, IEnumerable<IPayee> payees, IEnumerable<ICategory> categories);
        IEnumerable<ITitBase> GetAllTitsAsync(DateTime startTime, DateTime endTime, IAccount account = null);
        Task<long?> GetAccountBalanceAsync(IAccount account = null);
        IEnumerable<T> GetSubTransIncAsync<T>(long parentId) where T : class, ISubTransInc;
        Task ResetAsync();
    }

    public interface ICrudOrmAsync
    {
        Task<IEnumerable<T>> GetAllAsync<T>() where T : class, IDataModelBase;
        Task<int> InsertAsync<T>(T dataModelBase) where T : class, IDataModelBase;
        Task<T> GetAsync<T>(long id) where T : class, IDataModelBase;
        Task UpdateAsync<T>(T dataModelBase) where T : class, IDataModelBase;
        Task DeleteAsync<T>(T dataModelBase) where T : class, IDataModelBase;
    }
    
    public interface IPagedOrmAsync
    {
        Task<int> GetCountAsync<T>(object specifyingObject = null);
        Task<IEnumerable<T>> GetPageAsync<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
