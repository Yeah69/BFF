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
        PayeeViewModelService PayeeViewModelService { get; }
        ObservableCollection<IAccount> Accounts { get; }
        IObservableReadOnlyList<IAccountViewModel> AllAccountViewModels { get; }
        IObservableReadOnlyList<ICategoryViewModel> AllCategoryViewModels { get; }
        ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get;  }
        IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }
        bool IsValidToInsert(ICategoryViewModel categoryViewModel);
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        public AccountViewModelService AccountViewModelService { get; }
        
        public CategoryViewModelService CategoryViewModelService { get; }
        public PayeeViewModelService PayeeViewModelService { get; }

        public ObservableCollection<IAccount> Accounts { get;  }

        public IObservableReadOnlyList<IAccountViewModel> AllAccountViewModels { get; }

        public ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get; }

        public IObservableReadOnlyList<ICategoryViewModel> AllCategoryViewModels { get; }

        public IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }


        public CommonPropertyProvider(IBffOrm orm, BffRepository bffRepository)
        {
            
            Accounts = bffRepository.AccountRepository.All;
            AccountViewModelService = new AccountViewModelService(bffRepository.AccountRepository, orm);
            AllAccountViewModels = AccountViewModelService.All;
            AccountViewModelService.SummaryAccountViewModel.RefreshStartingBalance();
            
            CategoryViewModelService = new CategoryViewModelService(bffRepository.CategoryRepository, orm);
            AllCategoryViewModels = CategoryViewModelService.All;

            PayeeViewModelService = new PayeeViewModelService(bffRepository.PayeeRepository, orm);
            AllPayeeViewModels = PayeeViewModelService.All;
        }

        public bool IsValidToInsert(ICategoryViewModel categoryViewModel) => 
            categoryViewModel.Parent.Value == null && ParentCategoryViewModels.All(cvm => cvm.Name != categoryViewModel.Name) ||
            categoryViewModel.Parent.Value.Categories.All(cvm => cvm.Name != categoryViewModel.Name);
    }
}
