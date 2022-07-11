using System.Collections.ObjectModel;
using BFF.Core.IoC;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface IEditCategoriesViewModel
    {
        ReadOnlyObservableCollection<ICategoryViewModel> AllCategories { get; }

        ReadOnlyObservableCollection<IIncomeCategoryViewModel> AllIncomeCategories { get; }

        INewCategoryViewModel NewCategoryViewModel { get; }
    }

    public class EditCategoriesViewModel : ViewModelBase, IEditCategoriesViewModel, IScopeInstance
    {
        public INewCategoryViewModel NewCategoryViewModel { get; }

        public ReadOnlyObservableCollection<ICategoryViewModel> AllCategories { get; }

        public ReadOnlyObservableCollection<IIncomeCategoryViewModel> AllIncomeCategories { get; }

        public EditCategoriesViewModel(
            ICategoryViewModelService categoryService,
            IIncomeCategoryViewModelService incomeService,
            INewCategoryViewModel newCategoryViewModel)
        {
            NewCategoryViewModel = newCategoryViewModel;
            AllCategories = categoryService.All.ToReadOnlyObservableCollection();
            AllIncomeCategories = incomeService.All.ToReadOnlyObservableCollection();
        }
    }
}
