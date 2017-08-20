using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        private readonly BffRepository _bffRepository;
        
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        public ParentTransactionViewModelService ParentTransactionViewModelService { get; }

        public ParentIncomeViewModelService ParentIncomeViewModelService { get; }

        public SubTransactionViewModelService SubTransactionViewModelService { get; }

        public SubIncomeViewModelService SubIncomeViewModelService { get; }

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

        public SqLiteBffOrm(IProvideConnection provideConnection)
        {
            _bffRepository = new DapperBffRepository(provideConnection);
            
            CommonPropertyProvider = new CommonPropertyProvider(this, _bffRepository);

            SubTransactionViewModelService = new SubTransactionViewModelService(
                st => new SubTransactionViewModel(st, this, CommonPropertyProvider.CategoryViewModelService));

            SubIncomeViewModelService = new SubIncomeViewModelService(
                si => new SubIncomeViewModel(si, this, CommonPropertyProvider.CategoryViewModelService));

            ParentTransactionViewModelService = new ParentTransactionViewModelService(
                pt => new ParentTransactionViewModel(pt, this, SubTransactionViewModelService));
            ParentIncomeViewModelService = new ParentIncomeViewModelService(
                pi => new ParentIncomeViewModel(pi, this, SubIncomeViewModelService));
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
            => _bffRepository.TitRepository.GetPage(offset, pageSize, specifyingObject as Account) as IEnumerable<T>;

        public int GetCount<T>(object specifyingObject = null) => 
            _bffRepository.TitRepository.GetCount(specifyingObject as Account);

        public Task<IEnumerable<T>> GetPageAsync<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
            => _bffRepository.TitRepository.GetPageAsync(offset, pageSize, specifyingObject as Account) as Task<IEnumerable<T>>;

        public Task<int> GetCountAsync<T>(object specifyingObject = null) =>
            _bffRepository.TitRepository.GetCountAsync(specifyingObject as Account);
    }
}
