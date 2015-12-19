using System.Collections.Generic;
using BFF.Model.Native;
using BFF.Model.Native.Structure;

namespace BFF.DB
{
    public interface IDb
    {
        string DbPath { get; }
        void CreateNewDatabase();
        void OpenDatabase();
        void PopulateDatabase(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes, IEnumerable<SubIncome> subIncomes,
            IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees, IEnumerable<Category> categories);
        IEnumerable<TitBase> GetAllTits(Account account = null);
        long GetAccountBalance(Account account = null);
        IEnumerable<T> GetSubTransInc<T>(long parentId) where T : SubTransInc;
        IEnumerable<T> GetAll<T>() where T : DataModelBase;
        long Insert<T>(T dataModelBase) where T : DataModelBase;
        T Get<T>(long id) where T : DataModelBase;
        void Update<T>(T dataModelBase) where T : DataModelBase;
        void Delete<T>(T dataModelBase) where T : DataModelBase;
    }
}
