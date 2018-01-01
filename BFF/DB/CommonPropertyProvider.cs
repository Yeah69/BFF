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
        IAccountViewModelService AccountViewModelService { get; }
        ICategoryViewModelService CategoryViewModelService { get; }
        ICategoryBaseViewModelService CategoryBaseViewModelService { get; }
        IIncomeCategoryViewModelService IncomeCategoryViewModelService { get; }
        IPayeeViewModelService PayeeViewModelService { get; }
        IFlagViewModelService FlagViewModelService { get; }
        ObservableCollection<IAccount> Accounts { get; }
        IObservableReadOnlyList<IAccountViewModel> AllAccountViewModels { get; }
        IObservableReadOnlyList<ICategoryBaseViewModel> AllCategoryViewModels { get; }
        ObservableCollection<ICategoryBaseViewModel> ParentCategoryViewModels { get;  }
        IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }
    }

    public class CommonPropertyProvider : ICommonPropertyProvider
    {
        public IAccountViewModelService AccountViewModelService { get; }

        public ICategoryViewModelService CategoryViewModelService { get; }
        public IIncomeCategoryViewModelService IncomeCategoryViewModelService { get; }
        public ICategoryBaseViewModelService CategoryBaseViewModelService { get; }
        public IPayeeViewModelService PayeeViewModelService { get; }
        public IFlagViewModelService FlagViewModelService { get; }

        public ObservableCollection<IAccount> Accounts { get;  }

        public IObservableReadOnlyList<IAccountViewModel> AllAccountViewModels { get; }

        public ObservableCollection<ICategoryBaseViewModel> ParentCategoryViewModels { get; }

        public IObservableReadOnlyList<ICategoryBaseViewModel> AllCategoryViewModels { get; }

        public IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }


        public CommonPropertyProvider(IBffOrm orm, IBffRepository bffRepository)
        {
            
            Accounts = bffRepository.AccountRepository.All;
            AccountViewModelService = new AccountViewModelService(bffRepository.AccountRepository, orm);
            AllAccountViewModels = AccountViewModelService.All;
            AccountViewModelService.SummaryAccountViewModel.RefreshStartingBalance();

            CategoryViewModelService = new CategoryViewModelService(bffRepository.CategoryRepository, orm);
            IncomeCategoryViewModelService = new IncomeCategoryViewModelService(bffRepository.IncomeCategoryRepository, orm);
            AllCategoryViewModels = CategoryViewModelService.All;
            CategoryBaseViewModelService = new CategoryBaseViewModelService(CategoryViewModelService, IncomeCategoryViewModelService);

            PayeeViewModelService = new PayeeViewModelService(bffRepository.PayeeRepository, orm);
            FlagViewModelService = new FlagViewModelService(orm.BffRepository.FlagRepository, orm);
            AllPayeeViewModels = PayeeViewModelService.All;
        }
    }
}
