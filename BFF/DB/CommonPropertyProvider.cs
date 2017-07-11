using System.Collections.ObjectModel;
using System.Linq;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.DB
{
    public interface ICommonPropertyProvider 
    {
        AccountViewModelService AccountViewModelService { get; }
        CategoryViewModelService CategoryViewModelService { get; }
        ObservableCollection<Account> Accounts { get; }
        IObservableReadOnlyList<IAccountViewModel> AllAccountViewModels { get; }
        IObservableReadOnlyList<ICategoryViewModel> AllCategoryViewModels { get; }
        ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get;  }
        IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }
        IAccountViewModel GetAccountViewModel(long id);
        ICategoryViewModel GetCategoryViewModel(ICategory category);
        IPayeeViewModel GetPayeeViewModel(long id);
        bool IsValidToInsert(ICategoryViewModel categoryViewModel);
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        private readonly IBffOrm _orm;
        private readonly BffRepository _bffRepository;

        public AccountViewModelService AccountViewModelService { get; }
        
        public CategoryViewModelService CategoryViewModelService { get; }

        public ObservableCollection<Account> Accounts { get;  }

        public IObservableReadOnlyList<IAccountViewModel> AllAccountViewModels { get; }

        public ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get; }

        public IObservableReadOnlyList<ICategoryViewModel> AllCategoryViewModels { get; }

        public IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }


        public CommonPropertyProvider(IBffOrm orm, BffRepository bffRepository)
        {
            _orm = orm;
            _bffRepository = bffRepository;
            
            Accounts = _bffRepository.AccountRepository.All;
            AccountViewModelService = new AccountViewModelService(_bffRepository.AccountRepository, _orm);
            AllAccountViewModels = AccountViewModelService.All;
            AccountViewModelService.SummaryAccountViewModel.RefreshStartingBalance();
            
            AllPayeeViewModels = new TransformingObservableReadOnlyList<IPayee, IPayeeViewModel>
                (new WrappingObservableReadOnlyList<Payee>(_bffRepository.PayeeRepository.All), 
                 p => new PayeeViewModel(p, _orm));
            
            CategoryViewModelService = new CategoryViewModelService(_bffRepository.CategoryRepository, _orm);
            AllCategoryViewModels = CategoryViewModelService.All;
        }

        public IAccountViewModel GetAccountViewModel(long id)
            => AllAccountViewModels.Single(avm => avm.Id  == id);

        public ICategoryViewModel GetCategoryViewModel(ICategory category)
            => CategoryViewModelService.GetViewModel(category as Category);

        public IPayeeViewModel GetPayeeViewModel(long id)
            => AllPayeeViewModels.Single(pvm => pvm.Id  == id);

        public bool IsValidToInsert(ICategoryViewModel categoryViewModel) => 
            categoryViewModel.Parent.Value == null && ParentCategoryViewModels.All(cvm => cvm.Name != categoryViewModel.Name) ||
            categoryViewModel.Parent.Value.Categories.All(cvm => cvm.Name != categoryViewModel.Name);
    }
}
