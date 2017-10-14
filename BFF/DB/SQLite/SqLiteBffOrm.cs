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

        public IBffRepository BffRepository => _bffRepository;

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
                    hcvm => new NewCategoryViewModel(hcvm, _bffRepository.CategoryRepository, CommonPropertyProvider.CategoryViewModelService),
                    this, 
                    CommonPropertyProvider.CategoryViewModelService),
                () => BffRepository.SubTransactionRepository.Create());

            SubIncomeViewModelService = new SubIncomeViewModelService(
                si => new SubIncomeViewModel(
                    si,
                    hcvm => new NewCategoryViewModel(hcvm, _bffRepository.CategoryRepository, CommonPropertyProvider.CategoryViewModelService),
                    this,
                    CommonPropertyProvider.CategoryViewModelService),
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
