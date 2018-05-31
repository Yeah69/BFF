using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels
{
    public interface INewCategoryViewModel
    {
        IReactiveProperty<string> Name { get; }

        IReactiveProperty<ICategoryViewModel> Parent { get; }

        IReactiveProperty<bool> IsIncomeRelevant { get; }

        IReactiveProperty<int> MonthOffset { get; }

        ReactiveCommand AddCommand { get; }

        IObservableReadOnlyList<ICategoryViewModel> AllPotentialParents { get; }

        IObservableReadOnlyList<ICategoryBaseViewModel> All { get; }

        IHaveCategoryViewModel CurrentCategoryOwner { get; set; }
    }

    public sealed class NewCategoryViewModel : ViewModelBase, INewCategoryViewModel, IOncePerBackend, IDisposable
    {
        private readonly ICategoryViewModelService _categoryViewModelService;
        private readonly ICategoryBaseViewModelService _categoryBaseViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public NewCategoryViewModel(
            Func<ICategory, ICategory> categoryFactory,
            Func<IIncomeCategory> incomeCategoryFactory,
            ICategoryViewModelInitializer categoryViewModelInitializer,
            ICategoryViewModelService categoryViewModelService,
            IIncomeCategoryViewModelService incomeCategoryViewModelService,
            ICategoryBaseViewModelService categoryBaseViewModelService)
        {
            bool ValidateNewCategoryRelationCondition(string text, ICategoryViewModel parent)
            {
                if(IsIncomeRelevant?.Value ?? false)
                    return incomeCategoryViewModelService.All.All(icvm => icvm.Name.Value != text);
                return parent is null && AllPotentialParents.Where(cvw => cvw.Parent.Value is null).All(cvm => cvm.Name.Value != text) ||
                       parent != null && parent.Categories.All(cvm => cvm != null && cvm.Name.Value != text);
            }
            string ValidateNewCategoryRelationName(string text, ICategoryViewModel parent)
            {
                return ValidateNewCategoryRelationCondition(text, parent)
                        ? null
                        : "ErrorMessageWrongCategoryName".Localize();
            }
            string ValidateNewCategoryRelationParent(string text, ICategoryViewModel parent)
            {
                return ValidateNewCategoryRelationCondition(text, parent)
                    ? null
                    : "ErrorMessageWrongCategoryParent".Localize();
            }
            string ValidateNewCategoryName(string text)
            {
                return !string.IsNullOrWhiteSpace(text) 
                    ? null
                    : "ErrorMessageCategoryNameEmpty".Localize();
            }

            _categoryViewModelService = categoryViewModelService;
            _categoryBaseViewModelService = categoryBaseViewModelService;

            AddCommand = new ReactiveCommand();

            AddCommand.Where(_ =>
                {
                    (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                    (Name as ReactiveProperty<string>)?.ForceValidate();
                    return !Parent.HasErrors && !Name.HasErrors;
                })
                .Subscribe(async _ =>
                {
                    if (IsIncomeRelevant.Value)
                    {
                        IIncomeCategory newCategory = incomeCategoryFactory();
                        newCategory.Name = Name.Value.Trim();
                        newCategory.MonthOffset = MonthOffset.Value;
                        await newCategory.InsertAsync();
                        if(CurrentCategoryOwner != null)
                            CurrentCategoryOwner.Category.Value = incomeCategoryViewModelService.GetViewModel(newCategory);
                        CurrentCategoryOwner = null;
                    }
                    else
                    {
                        ICategory newCategory = categoryFactory(_categoryViewModelService.GetModel(Parent.Value));
                        newCategory.Name = Name.Value.Trim();
                        newCategory.Parent?.AddCategory(newCategory);
                        await newCategory.InsertAsync();
                        OnPropertyChanged(nameof(AllPotentialParents));
                        var categoryViewModel = _categoryViewModelService.GetViewModel(newCategory);
                        categoryViewModelInitializer.Initialize(categoryViewModel);
                        if (CurrentCategoryOwner != null)
                            CurrentCategoryOwner.Category.Value = categoryViewModel;
                        CurrentCategoryOwner = null;
                    }
                })
                .AddTo(_compositeDisposable);
            Name = new ReactiveProperty<string>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .SetValidateNotifyError(text =>
                {
                    string ret = ValidateNewCategoryName(text);
                    return ret ?? ValidateNewCategoryRelationName(text, Parent?.Value);
                })
                .AddTo(_compositeDisposable);
            Parent = new ReactiveProperty<ICategoryViewModel>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .SetValidateNotifyError(parent => ValidateNewCategoryRelationParent(Name.Value, parent))
                .AddTo(_compositeDisposable);

            IsIncomeRelevant = new ReactiveProperty<bool>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(_compositeDisposable);

            IsIncomeRelevant.Subscribe(_ =>
            {
                (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                (Name as ReactiveProperty<string>)?.ForceValidate();
            }).AddTo(_compositeDisposable);
       
            MonthOffset = new ReactiveProperty<int>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(_compositeDisposable);

            Name
                .Subscribe(_ => (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate())
                .AddTo(_compositeDisposable);
            Parent
                .Subscribe(_ => (Name as ReactiveProperty<string>)?.ForceValidate())
                .AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public IReactiveProperty<string> Name { get; }
        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Parent { get; }

        public IReactiveProperty<bool> IsIncomeRelevant { get; }
        public IReactiveProperty<int> MonthOffset { get; }

        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ReactiveCommand AddCommand { get; }

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IObservableReadOnlyList<ICategoryViewModel> AllPotentialParents => _categoryViewModelService.All;

        public IObservableReadOnlyList<ICategoryBaseViewModel> All => _categoryBaseViewModelService.All;

        public IHaveCategoryViewModel CurrentCategoryOwner { get; set; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}
