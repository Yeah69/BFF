using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Properties;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using WPFLocalizeExtension.Engine;

namespace BFF.MVVM.ViewModels
{
    public interface INewCategoryViewModel
    {
        IReactiveProperty<string> CategoryText { get; }

        IReactiveProperty<ICategoryViewModel> AddingCategoryParent { get; }

        ReactiveCommand AddCategoryCommand { get; }

        IObservableReadOnlyList<ICategoryViewModel> AllCategories { get; }
    }

    public sealed class NewCategoryViewModel : ObservableObject, INewCategoryViewModel, IDisposable
    {
        private readonly ICategoryViewModelService _categoryViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public NewCategoryViewModel(
            IHaveCategoryViewModel categoryOwner, 
            ICategoryRepository categoryRepository,
            ICategoryViewModelService categoryViewModelService)
        {
            bool ValidateNewCategoryRelationCondition(string text, ICategoryViewModel parent)
            {
                return parent == null && AllCategories.Where(cvw => cvw.Parent.Value == null).All(cvm => cvm.Name.Value != text) ||
                       parent != null && parent.Categories.All(cvm => cvm != null && cvm.Name.Value != text);
            }
            string ValidateNewCategoryRelationName(string text, ICategoryViewModel parent)
            {
                return ValidateNewCategoryRelationCondition(text, parent)
                        ? null
                        : "ErrorMessageWrongCategoryName".Localize<string>();
            }
            string ValidateNewCategoryRelationParent(string text, ICategoryViewModel parent)
            {
                return ValidateNewCategoryRelationCondition(text, parent)
                    ? null
                    : "ErrorMessageWrongCategoryParent".Localize<string>();
            }
            string ValidateNewCategoryName(string text)
            {
                return !string.IsNullOrWhiteSpace(text) 
                    ? null
                    : "ErrorMessageCategoryNameEmpty".Localize<string>();
            }

            _categoryViewModelService = categoryViewModelService;

            AddCategoryCommand = new ReactiveCommand();

            AddCategoryCommand.Where(_ =>
                {
                    (AddingCategoryParent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                    (CategoryText as ReactiveProperty<string>)?.ForceValidate();
                    return !AddingCategoryParent.HasErrors && !CategoryText.HasErrors;
                })
                .Subscribe(_ =>
                {
                    ICategory newCategory = categoryRepository.Create();
                    newCategory.Name = CategoryText.Value.Trim();
                    newCategory.Parent = _categoryViewModelService.GetModel(AddingCategoryParent.Value);
                    newCategory.Parent?.AddCategory(newCategory);
                    newCategory.Insert();
                    OnPropertyChanged(nameof(AllCategories));
                    categoryOwner.Category.Value = _categoryViewModelService.GetViewModel(newCategory);
                })
                .AddTo(_compositeDisposable);
            CategoryText = new ReactiveProperty<string>()
                .SetValidateNotifyError(text =>
                {
                    string ret = ValidateNewCategoryName(text);
                    return ret ?? ValidateNewCategoryRelationName(text, AddingCategoryParent?.Value);
                })
                .AddTo(_compositeDisposable);
            AddingCategoryParent = new ReactiveProperty<ICategoryViewModel>()
                .SetValidateNotifyError(parent => ValidateNewCategoryRelationParent(CategoryText.Value, parent))
                .AddTo(_compositeDisposable);

            CategoryText
                .Subscribe(_ => (AddingCategoryParent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate())
                .AddTo(_compositeDisposable);
            AddingCategoryParent
                .Subscribe(_ => (CategoryText as ReactiveProperty<string>)?.ForceValidate())
                .AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public IReactiveProperty<string> CategoryText { get; }
        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> AddingCategoryParent { get; }

        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ReactiveCommand AddCategoryCommand { get; }

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IObservableReadOnlyList<ICategoryViewModel> AllCategories => _categoryViewModelService.All;

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}
