using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native;
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

        public ISubTransactionViewModelService SubTransactionViewModelService { get; }

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

        public IEnumerable<ISubTransaction> GetSubTransInc(long parentId)
        {
            return _bffRepository.SubTransactionRepository?.GetChildrenOf(parentId) 
                    ?? Enumerable.Empty<ISubTransaction>();
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
                        CommonPropertyProvider.IncomeCategoryViewModelService,
                        CommonPropertyProvider.CategoryBaseViewModelService),
                    this, 
                    CommonPropertyProvider.CategoryBaseViewModelService),
                () => BffRepository.SubTransactionRepository.Create());

            ParentTransactionViewModelService = new ParentTransactionViewModelService(
                pt => new ParentTransactionViewModel(
                    pt,
                    hpvm => new NewPayeeViewModel(hpvm, _bffRepository.PayeeRepository, CommonPropertyProvider.PayeeViewModelService), 
                    this,
                    SubTransactionViewModelService,
                    CommonPropertyProvider.FlagViewModelService));

            BudgetEntryViewModelService = new BudgetEntryViewModelService(be => new BudgetEntryViewModel(this, be, CommonPropertyProvider.CategoryViewModelService));
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
            => _bffRepository.TransViewRepository.GetPage(offset, pageSize, specifyingObject as IAccount) as IEnumerable<T>;

        public int GetCount<T>(object specifyingObject = null) => 
            _bffRepository.TransViewRepository.GetCount(specifyingObject as IAccount);

        public void Dispose()
        {
            _bffRepository?.Dispose();
        }
    }
}
