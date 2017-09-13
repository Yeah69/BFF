using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    public interface INewCategoryViewModel
    {
        IReactiveProperty<string> CategoryText { get; }

        IReactiveProperty<ICategoryViewModel> AddingCategoryParent { get; }

        ReactiveCommand AddCategoryCommand { get; }

        IObservableReadOnlyList<ICategoryViewModel> AllCategories { get; }
    }

    public sealed class NewCategoryViewModel : ObservableObject, INewCategoryViewModel
    {
        private readonly ICategoryViewModelService _categoryViewModelService;

        public NewCategoryViewModel(
            IHaveCategoryViewModel categoryOwner, 
            ICategoryRepository categoryRepository,
            ICategoryViewModelService categoryViewModelService)
        {
            _categoryViewModelService = categoryViewModelService;


            var observeCollection = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(AllCategories, "CollectionChanged");

            AddCategoryCommand = CategoryText.CombineLatest(AddingCategoryParent, observeCollection, (text, parent, _) =>
                !string.IsNullOrWhiteSpace(text) &&
                (parent == null && AllCategories.Where(cvw => cvw.Parent.Value == null).All(cvm => cvm.Name.Value != text) ||
                 parent != null && parent.Categories.All(cvm => cvm != null && cvm.Name.Value != text))).ToReactiveCommand();

            AddCategoryCommand.Where(_ => !string.IsNullOrWhiteSpace(CategoryText.Value)).Subscribe(_ =>
            {
                ICategory newCategory = categoryRepository.Create();
                newCategory.Name = CategoryText.Value.Trim();
                newCategory.Parent = _categoryViewModelService.GetModel(AddingCategoryParent.Value);
                newCategory.Parent.AddCategory(newCategory);
                newCategory.Insert();
                OnPropertyChanged(nameof(AllCategories));
                categoryOwner.Category.Value = _categoryViewModelService.GetViewModel(newCategory);
            });
        }

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public IReactiveProperty<string> CategoryText { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> AddingCategoryParent { get; } = new ReactiveProperty<ICategoryViewModel>();

        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ReactiveCommand AddCategoryCommand { get; }

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IObservableReadOnlyList<ICategoryViewModel> AllCategories => _categoryViewModelService.All;
    }
}
