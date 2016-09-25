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

        public Task<IEnumerable<T>> GetAllAsync<T>() where T : class, IDataModelBase
        {
            Task<IEnumerable<T>> ret;
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                ret = cnn.OpenAndReturn().GetAllAsync<T>();
                cnn.Close();
            }
            return ret;
        }

        public Task<int> InsertAsync<T>(T dataModelBase) where T : class, IDataModelBase
        {
            Task<int> ret;
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                ret = cnn.OpenAndReturn().InsertAsync(dataModelBase);
                ret.GetAwaiter().OnCompleted(() => dataModelBase.Id = ret.Result);
                cnn.Close();
            }
            return ret;
        }

        public Task<T> GetAsync<T>(long id) where T : class, IDataModelBase
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync<T>(T dataModelBase) where T : class, IDataModelBase
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(T dataModelBase) where T : class, IDataModelBase
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

        public Task PopulateDatabaseAsync(IEnumerable<ITransaction> transactions, IEnumerable<ISubTransaction> subTransactions, IEnumerable<IIncome> incomes,
                                          IEnumerable<ISubIncome> subIncomes, IEnumerable<ITransfer> transfers, IEnumerable<IAccount> accounts, IEnumerable<IPayee> payees,
                                          IEnumerable<ICategory> categories)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITitBase> GetAllTitsAsync(DateTime startTime, DateTime endTime, IAccount account = null)
        {
            throw new NotImplementedException();
        }

        public Task<long?> GetAccountBalanceAsync(IAccount account = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetSubTransIncAsync<T>(long parentId) where T : class, ISubTransInc
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
