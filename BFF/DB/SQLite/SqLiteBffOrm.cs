using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        private readonly BffRepository _bffRepository;
        
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        public BffRepository BffRepository => _bffRepository;

        public long? GetAccountBalance(IAccount account) => 
            _bffRepository.AccountRepository?.GetBalance(account);

        public long? GetSummaryAccountBalance() =>
            _bffRepository.AccountRepository?.GetBalance(
                new SummaryAccount(_bffRepository.AccountRepository));

        public IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc
        {
            if(typeof(T) == typeof(SubIncome))
                return _bffRepository.SubIncomeRepository?.GetChildrenOf(parentId) 
                       ?? Enumerable.Empty<ISubIncome>();
            if(typeof(T) == typeof(SubTransaction))
                return _bffRepository.SubTransactionRepository?.GetChildrenOf(parentId) 
                       ?? Enumerable.Empty<ISubTransaction>();
            return Enumerable.Empty<ISubTransInc>();
        }

        private IRepository<T> GetRepository<T>() where T : class, IDataModel
        {
            if(typeof(T) == typeof(Account))
                return _bffRepository.AccountRepository as IRepository<T>;
            if(typeof(T) == typeof(BudgetEntry))
                return _bffRepository.BudgetEntryRepository as IRepository<T>;
            if(typeof(T) == typeof(Category))
                return _bffRepository.CategoryRepository as IRepository<T>;
            if(typeof(T) == typeof(DbSetting))
                return _bffRepository.DbSettingRepository as IRepository<T>;
            if(typeof(T) == typeof(Income))
                return _bffRepository.IncomeRepository as IRepository<T>;
            if(typeof(T) == typeof(ParentIncome))
                return _bffRepository.ParentIncomeRepository as IRepository<T>;
            if(typeof(T) == typeof(ParentTransaction))
                return _bffRepository.ParentTransactionRepository as IRepository<T>;
            if(typeof(T) == typeof(Payee))
                return _bffRepository.PayeeRepository as IRepository<T>;
            if(typeof(T) == typeof(SubIncome))
                return _bffRepository.SubIncomeRepository as IRepository<T>;
            if(typeof(T) == typeof(SubTransaction))
                return _bffRepository.SubTransactionRepository as IRepository<T>;
            if(typeof(T) == typeof(Transaction))
                return _bffRepository.TransactionRepository as IRepository<T>;
            if(typeof(T) == typeof(Transfer))
                return _bffRepository.TransferRepository as IRepository<T>;

            return null;
        }


        public void Insert<T>(T dataModelBase) where T : class, IDataModel => GetRepository<T>().Add(dataModelBase);

        public T Get<T>(long id) where T : class, IDataModel => GetRepository<T>().Find(id);

        public void Update<T>(T dataModelBase) where T : class, IDataModel => GetRepository<T>().Update(dataModelBase);

        public void Delete<T>(T dataModelBase) where T : class, IDataModel => GetRepository<T>().Delete(dataModelBase);

        public SqLiteBffOrm(IProvideConnection provideConnection)
        {
            _bffRepository = new DapperBffRepository(provideConnection);
            
            CommonPropertyProvider = new CommonPropertyProvider(this, _bffRepository);
            ((CommonPropertyProvider) CommonPropertyProvider).InitializeCategoryViewModels();
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
            => _bffRepository.TitRepository.GetPage(offset, pageSize, specifyingObject as Account) as IEnumerable<T>;

        public int GetCount<T>(object specifyingObject = null) => 
            _bffRepository.TitRepository.GetCount(specifyingObject as Account);
    }
}
