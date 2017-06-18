using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModelRepositories;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Helpers;

namespace BFF.DB
{
    public interface ICommonPropertyProvider 
    {
        CategoryViewModelService CategoryViewModelService { get; }
        ObservableCollection<IAccount> Accounts { get; }
        ObservableCollection<IAccountViewModel> AllAccountViewModels { get; }
        ISummaryAccountViewModel SummaryAccountViewModel { get; }
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

        public CategoryViewModelService CategoryViewModelService { get; }

        public ObservableCollection<IAccount> Accounts { get;  }

        public ObservableCollection<IAccountViewModel> AllAccountViewModels { get; }

        public ISummaryAccountViewModel SummaryAccountViewModel { get; }

        public ObservableCollection<ICategoryViewModel> ParentCategoryViewModels { get; private set; }

        public IObservableReadOnlyList<ICategoryViewModel> AllCategoryViewModels { get; }

        public IObservableReadOnlyList<IPayeeViewModel> AllPayeeViewModels { get; }


        public CommonPropertyProvider(IBffOrm orm, BffRepository bffRepository)
        {
            _orm = orm;
            _bffRepository = bffRepository;
            CategoryViewModelService = new CategoryViewModelService(_bffRepository.CategoryRepository, _orm);
            
            SummaryAccountViewModel = new SummaryAccountViewModel(
                orm, new SummaryAccount(_bffRepository.AccountRepository));
            
            Accounts = new ObservableCollection<IAccount>(_bffRepository.AccountRepository.FindAll());
            
            AllAccountViewModels = new ObservableCollection<IAccountViewModel>(Accounts.Select(a => new AccountViewModel(a, orm)));
            AllPayeeViewModels = new TransformingObservableReadOnlyList<IPayee, IPayeeViewModel>
                (new WrappingObservableReadOnlyList<Payee>(_bffRepository.PayeeRepository.All), 
                 p => new PayeeViewModel(p, _orm));
            AllCategoryViewModels = CategoryViewModelService.All;
        }

        public IAccountViewModel GetAccountViewModel(long id)
            => AllAccountViewModels.Single(avm => avm.Id  == id);

        public ICategoryViewModel GetCategoryViewModel(ICategory category)
            => CategoryViewModelService.GetViewModel(category);

        public IPayeeViewModel GetPayeeViewModel(long id)
            => AllPayeeViewModels.Single(pvm => pvm.Id  == id);

        public bool IsValidToInsert(ICategoryViewModel categoryViewModel) => 
            categoryViewModel.Parent.Value == null && ParentCategoryViewModels.All(cvm => cvm.Name != categoryViewModel.Name) ||
            categoryViewModel.Parent.Value.Categories.All(cvm => cvm.Name != categoryViewModel.Name);
    }
}
