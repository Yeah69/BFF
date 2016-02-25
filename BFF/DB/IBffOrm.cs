using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BFF.Model.Native;
using BFF.Model.Native.Structure;

namespace BFF.DB
{
    public interface IBffOrm : ICrudOrm, IPeripheryProvider, IPagedOrm
    {
        string DbPath { get; set; }
        event PropertyChangedEventHandler DbPathChanged;
        void CreateNewDatabase();
        void PopulateDatabase(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes, IEnumerable<SubIncome> subIncomes,
            IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees, IEnumerable<Category> categories);
        IEnumerable<TitBase> GetAllTits(DateTime startTime, DateTime endTime, Account account = null);
        long? GetAccountBalance(Account account = null);
        IEnumerable<T> GetSubTransInc<T>(long parentId) where T : SubTitBase;
        void Reset();
    }

    public interface ICrudOrm
    {
        IEnumerable<T> GetAll<T>() where T : DataModelBase;
        long Insert<T>(T dataModelBase) where T : DataModelBase;
        T Get<T>(long id) where T : DataModelBase;
        void Update<T>(T dataModelBase) where T : DataModelBase;
        void Delete<T>(T dataModelBase) where T : DataModelBase;
    }

    public interface IPeripheryProvider
    {
        ObservableCollection<Account> AllAccounts { get; }
        ObservableCollection<Payee> AllPayees { get; }
        ObservableCollection<Category> AllCategories { get; }
        Account GetAccount(long id);
        Payee GetPayee(long id);
        Category GetCategory(long id);
    }
    
    public interface IPagedOrm
    {
        int GetCount<T>(object specifyingObject = null);
        IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
