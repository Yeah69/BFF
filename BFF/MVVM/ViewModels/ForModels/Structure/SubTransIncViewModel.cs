using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ISubTransIncViewModel : ITitLikeViewModel
    {
        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        IReactiveProperty<ICategoryViewModel> Category { get; }
    }

    /// <summary>
    /// Base class for ViewModels of the Models SubTransaction and SubIncome
    /// </summary>
    public abstract class SubTransIncViewModel : TitLikeViewModel, ISubTransIncViewModel
    {
        private readonly CategoryViewModelService _categoryViewModelService;

        #region SubTransaction/SubIncome Properties

        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Category { get; }

        /// <summary>
        /// The amount of money of the exchange of the SubTransaction or SubIncome.
        /// </summary>
        public override IReactiveProperty<long> Sum { get;  }

        #endregion

        /// <summary>
        /// Initializes a SubTransIncViewModel.
        /// </summary>
        /// <param name="subIncome">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="categoryViewModelService">Service for categories.</param>
        protected SubTransIncViewModel(ISubTransInc subIncome, IBffOrm orm, CategoryViewModelService categoryViewModelService) : base(orm, subIncome)
        {
            _categoryViewModelService = categoryViewModelService;

            Category = subIncome.ToReactivePropertyAsSynchronized(
                sti => sti.Category,
                categoryViewModelService.GetViewModel, 
                categoryViewModelService.GetModel).AddTo(CompositeDisposable);
            Sum = subIncome.ToReactivePropertyAsSynchronized(sti => sti.Sum).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Category != null;
        }

        #region Category Editing

        //todo: Delegate the Editing?

        private string _categoryText;

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string CategoryText
        {
            get => _categoryText;
            set
            {
                _categoryText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public ICategoryViewModel AddingCategoryParent { get; set; }

        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            ICategory newCategory = Orm.BffRepository.CategoryRepository.Create();
            newCategory.Name = CategoryText.Trim();
            newCategory.Parent = CommonPropertyProvider.CategoryViewModelService.GetModel(AddingCategoryParent);
            newCategory.Insert();
            OnPropertyChanged(nameof(AllCategories));
            Category.Value = _categoryViewModelService.GetViewModel(newCategory);
        }, obj =>
        {
            return !string.IsNullOrWhiteSpace(CategoryText) &&
            (AddingCategoryParent == null && (CommonPropertyProvider?.ParentCategoryViewModels.All(pcvm => pcvm.Name.Value != CategoryText) ?? false) ||
            AddingCategoryParent != null && AddingCategoryParent.Categories.All(c => c.Name.Value != CategoryText));
        });

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IObservableReadOnlyList<ICategoryViewModel> AllCategories => CommonPropertyProvider?.AllCategoryViewModels;

        #endregion
    }
}
