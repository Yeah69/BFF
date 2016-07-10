using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using BFF.Model.Native;
using BFF.Model.Native.Structure;

namespace BFF.DB
{
    public interface IBffOrmAsync : ICrudOrmAsync, IPeripheryProvider, IPagedOrmAsync
    {
        string DbPath { get; set; }
        event PropertyChangedEventHandler DbPathChanged;
        Task CreateNewDatabaseAsync(string dbPath);
        Task PopulateDatabaseAsync(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes, IEnumerable<SubIncome> subIncomes,
            IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees, IEnumerable<Category> categories);
        IEnumerable<TitBase> GetAllTitsAsync(DateTime startTime, DateTime endTime, Account account = null);
        Task<long?> GetAccountBalanceAsync(Account account = null);
        IEnumerable<T> GetSubTransIncAsync<T>(long parentId) where T : SubTitBase;
        Task ResetAsync();
    }

    public interface ICrudOrmAsync
    {
        Task<IEnumerable<T>> GetAllAsync<T>() where T : DataModelBase;
        Task<int> InsertAsync<T>(T dataModelBase) where T : DataModelBase;
        Task<T> GetAsync<T>(long id) where T : DataModelBase;
        Task UpdateAsync<T>(T dataModelBase) where T : DataModelBase;
        Task DeleteAsync<T>(T dataModelBase) where T : DataModelBase;
    }
    
    public interface IPagedOrmAsync
    {
        Task<int> GetCountAsync<T>(object specifyingObject = null);
        Task<IEnumerable<T>> GetPageAsync<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
