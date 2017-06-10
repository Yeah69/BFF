using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using DbTransactions = System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using NLog;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        private BffRepository _bffRepository;
        
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string DbPath { get; }

        protected string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments)
        {
            _bffRepository.PopulateDatabase(importLists, importAssignments);
        }

        public long? GetAccountBalance(IAccount account) => 
            (_bffRepository.AccountRepository as AccountRepository)?.GetBalance(account);

        public long? GetSummaryAccountBalance() =>
            (_bffRepository.AccountRepository as AccountRepository)?.GetBalance(new SummaryAccount());

        public IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc
        {
            Logger.Debug("Getting SubTransactions/SubIncomes from table {0} with the ParentId {1}.", typeof(T).Name, parentId);
            IEnumerable<ISubTransInc> ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                string query = $"SELECT * FROM [{typeof(T).Name}s] WHERE {nameof(ISubTransInc.ParentId)} = @id;";
                ret = cnn.Query<T>(query, new { id = parentId }).Cast<ISubTransInc>();

                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<T> GetAll<T>() where T : class, IDataModel
        {
            Logger.Debug("Getting all entries from table {0}.", typeof(T).Name);
            IEnumerable<T> ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                ret = cnn.GetAll<T>();
                transactionScope.Complete();
            }
            return ret;
        }

        public void Insert<T>(T dataModelBase) where T : class, IDataModel
        {
            Logger.Debug("Insert an entry into table {0}.", typeof(T).Name);
            long ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                ret = cnn.Insert(dataModelBase);
                dataModelBase.Id = ret;
                transactionScope.Complete();
            }
        }

        public T Get<T>(long id) where T : class, IDataModel
        {
            T ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                ret = cnn.Get<T>(id);
                transactionScope.Complete();
            }
            return ret;
        }
        
        public void Update<T>(T dataModelBase) where T : class, IDataModel
        {
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                cnn.Update<T>(dataModelBase);
                transactionScope.Complete();
            }
        }

        public void Delete<T>(T dataModelBase) where T : class, IDataModel
        {
            Logger.Debug("Delete an entry from table {0}.", typeof(T).Name);
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                cnn.Delete(dataModelBase);
                transactionScope.Complete();
            }
        }

        public SqLiteBffOrm(string dbPath, IProvideConnection provideConnection)
        {
            DbPath = dbPath;
            _bffRepository = new DapperBffRepository(provideConnection);
            
            CommonPropertyProvider = new CommonPropertyProvider(this, _bffRepository);
            (CommonPropertyProvider as CommonPropertyProvider).InitializeCategoryViewModels();
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
        {
            return _bffRepository.TitRepository.GetPage(offset, pageSize, specifyingObject as Account) as IEnumerable<T>;
        }

        public int GetCount<T>(object specifyingObject = null)
        {
            return _bffRepository.TitRepository.GetCount(specifyingObject as Account);
        }
    }
}
