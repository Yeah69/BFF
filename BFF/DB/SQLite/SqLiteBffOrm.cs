using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        private readonly IBffRepository _bffRepository;
        
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        public IParentTransactionViewModelService ParentTransactionViewModelService { get; }

        public IParentIncomeViewModelService ParentIncomeViewModelService { get; }

        public ISubTransactionViewModelService SubTransactionViewModelService { get; }

        public ISubIncomeViewModelService SubIncomeViewModelService { get; }

        public IBudgetEntryViewModelService BudgetEntryViewModelService { get; }

        public IBffRepository BffRepository => _bffRepository;

        public long? GetAccountBalanceUntilNow(IAccount account) =>
            _bffRepository.AccountRepository?.GetBalanceUntilNow(account);

        public long? GetSummaryAccountBalanceUntilNow() =>
            _bffRepository.AccountRepository?.GetBalanceUntilNow(
                new SummaryAccount(_bffRepository.AccountRepository));

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
                st => new SubTransactionViewModel(
                    st,
                    hcvm => new NewCategoryViewModel(
                        hcvm,
                        _bffRepository.CategoryRepository,
                        _bffRepository.IncomeCategoryRepository, 
                        CommonPropertyProvider.CategoryViewModelService,
                        CommonPropertyProvider.IncomeCategoryViewModelService),
                    this, 
                    CommonPropertyProvider.CategoryBaseViewModelService),
                () => BffRepository.SubTransactionRepository.Create());

            SubIncomeViewModelService = new SubIncomeViewModelService(
                si => new SubIncomeViewModel(
                    si,
                    hcvm => new NewCategoryViewModel(
                        hcvm, 
                        _bffRepository.CategoryRepository,
                        _bffRepository.IncomeCategoryRepository,
                        CommonPropertyProvider.CategoryViewModelService,
                        CommonPropertyProvider.IncomeCategoryViewModelService),
                    this,
                    CommonPropertyProvider.CategoryBaseViewModelService),
                () => BffRepository.SubIncomeRepository.Create());

            ParentTransactionViewModelService = new ParentTransactionViewModelService(
                pt => new ParentTransactionViewModel(
                    pt,
                    hpvm => new NewPayeeViewModel(hpvm, _bffRepository.PayeeRepository, CommonPropertyProvider.PayeeViewModelService), 
                    this,
                    SubTransactionViewModelService));
            ParentIncomeViewModelService = new ParentIncomeViewModelService(
                pi => new ParentIncomeViewModel(
                    pi,
                    hpvm => new NewPayeeViewModel(hpvm, _bffRepository.PayeeRepository, CommonPropertyProvider.PayeeViewModelService),
                    this,
                    SubIncomeViewModelService));

            BudgetEntryViewModelService = new BudgetEntryViewModelService(be => new BudgetEntryViewModel(this, be, CommonPropertyProvider.CategoryViewModelService));
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
            => _bffRepository.TitRepository.GetPage(offset, pageSize, specifyingObject as IAccount) as IEnumerable<T>;

        public int GetCount<T>(object specifyingObject = null) => 
            _bffRepository.TitRepository.GetCount(specifyingObject as IAccount);

        public void Dispose()
        {
            _bffRepository?.Dispose();
        }
    }
}
