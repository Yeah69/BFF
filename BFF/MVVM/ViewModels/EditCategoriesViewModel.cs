using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public interface IEditCategoriesViewModel
    {
        ReadOnlyObservableCollection<ICategoryViewModel> AllCategories { get; }

        ReadOnlyObservableCollection<IIncomeCategoryViewModel> AllIncomeCategories { get; }

        INewCategoryViewModel NewCategoryViewModel { get; }
    }

    public class EditCategoriesViewModel : ViewModelBase, IEditCategoriesViewModel, IOncePerBackend
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
