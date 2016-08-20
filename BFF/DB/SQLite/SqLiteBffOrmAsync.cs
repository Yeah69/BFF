using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrmAsync : SqLiteBffOrm, IBffOrmAsync
    {
        #region Implementation of ICrudOrmAsync

        public Task<IEnumerable<T>> GetAllAsync<T>() where T : DataModelBase
        {
            if (!DbLockFlag)
            {
                Task<IEnumerable<T>> ret;
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    ret = cnn.OpenAndReturn().GetAllAsync<T>();
                    cnn.Close();
                }
                return ret;
            }
            Task<IEnumerable<T>> newTask = new Task<IEnumerable<T>>(() => new List<T>());
            newTask.Start();
            return newTask;
        }

        public Task<int> InsertAsync<T>(T dataModelBase) where T : DataModelBase
        {
            Task<int> ret = null;
            if (!DbLockFlag)
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    ret = cnn.OpenAndReturn().InsertAsync(dataModelBase);
                    ret.GetAwaiter().OnCompleted(() => dataModelBase.Id = ret.Result);
                    cnn.Close();
                }
            }
            return ret;
        }

        public Task<T> GetAsync<T>(long id) where T : DataModelBase
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync<T>(T dataModelBase) where T : DataModelBase
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(T dataModelBase) where T : DataModelBase
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IPagedOrmAsync

        public Task<int> GetCountAsync<T>(object specifyingObject = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetPageAsync<T>(int offset, int pageSize, object specifyingObject = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IBffOrmAsync

        public Task CreateNewDatabaseAsync(string dbPath)
        {
            throw new NotImplementedException();
        }

        public Task PopulateDatabaseAsync(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes,
                                          IEnumerable<SubIncome> subIncomes, IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees,
                                          IEnumerable<Category> categories)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TitBase> GetAllTitsAsync(DateTime startTime, DateTime endTime, Account account = null)
        {
            throw new NotImplementedException();
        }

        public Task<long?> GetAccountBalanceAsync(Account account = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetSubTransIncAsync<T>(long parentId) where T : SubTransInc
        {
            throw new NotImplementedException();
        }

        public Task ResetAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        public SqLiteBffOrmAsync(string dbPath) : base(dbPath) {}
    }
}
