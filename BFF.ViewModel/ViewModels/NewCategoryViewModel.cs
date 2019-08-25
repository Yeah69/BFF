using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface INewCategoryViewModel
    {
        string Name { get; set; }

        IReactiveProperty<ICategoryViewModel> Parent { get; }

        IReactiveProperty<bool> IsIncomeRelevant { get; }

        IReactiveProperty<int> MonthOffset { get; }

        ICommand AddCommand { get; }

        ICommand DeselectParentCommand { get; }

        IObservableReadOnlyList<ICategoryViewModel> AllPotentialParents { get; }

        IObservableReadOnlyList<ICategoryBaseViewModel> All { get; }

        IHaveCategoryViewModel CurrentCategoryOwner { get; set; }
    }

    internal sealed class NewCategoryViewModel : NotifyingErrorViewModelBase, INewCategoryViewModel, IOncePerBackend, IDisposable
    {
        private readonly ILocalizer _localizer;
        private readonly ICategoryViewModelService _categoryViewModelService;
        private readonly IIncomeCategoryViewModelService _incomeCategoryViewModelService;
        private readonly ICategoryBaseViewModelService _categoryBaseViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private string _name;

        public NewCategoryViewModel(
            Func<ICategory, ICategory> categoryFactory,
            Func<IIncomeCategory> incomeCategoryFactory,
            ILocalizer localizer,
            ICategoryViewModelInitializer categoryViewModelInitializer,
            ICategoryViewModelService categoryViewModelService,
            IIncomeCategoryViewModelService incomeCategoryViewModelService,
            IBudgetOverviewViewModel budgetOverviewViewModel,
            ICategoryBaseViewModelService categoryBaseViewModelService)
        {
            bool ValidateNewCategoryRelationCondition(string text, ICategoryViewModel parent)
            {
                if(IsIncomeRelevant?.Value ?? false)
                    return incomeCategoryViewModelService.All.All(icvm => icvm.Name != text);
                return parent is null && AllPotentialParents.Where(cvw => cvw.Parent is null).All(cvm => cvm.Name != text) ||
                       parent != null && parent.Categories.All(cvm => cvm != null && cvm.Name != text);
            }
            string ValidateNewCategoryRelationParent(string text, ICategoryViewModel parent)
            {
                return ValidateNewCategoryRelationCondition(text, parent)
                    ? null
                    : _localizer.Localize("ErrorMessageWrongCategoryParent");
            }

            _localizer = localizer;
            _categoryViewModelService = categoryViewModelService;
            _incomeCategoryViewModelService = incomeCategoryViewModelService;
            _categoryBaseViewModelService = categoryBaseViewModelService;

            AddCommand = new AsyncRxRelayCommand(async () =>
                {
                    (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                    if(Parent.HasErrors || !ValidateName()) return;

                    if (IsIncomeRelevant.Value)
                    {
                        IIncomeCategory newCategory = incomeCategoryFactory();
                        newCategory.Name = Name.Trim();
                        newCategory.MonthOffset = MonthOffset.Value;
                        await newCategory.InsertAsync();
                        if (CurrentCategoryOwner != null)
                            CurrentCategoryOwner.Category = incomeCategoryViewModelService.GetViewModel(newCategory);
                        CurrentCategoryOwner = null;
                    }
                    else
                    {
                        ICategory newCategory = categoryFactory(_categoryViewModelService.GetModel(Parent.Value));
                        newCategory.Name = Name.Trim();
                        newCategory.Parent?.AddCategory(newCategory);
                        await newCategory.InsertAsync();
                        OnPropertyChanged(nameof(AllPotentialParents));
                        var categoryViewModel = _categoryViewModelService.GetViewModel(newCategory);
                        categoryViewModelInitializer.Initialize(categoryViewModel);
                        if (CurrentCategoryOwner != null)
                            CurrentCategoryOwner.Category = categoryViewModel;
                        CurrentCategoryOwner = null;
                    }
                    _name = "";
                    OnPropertyChanged(nameof(Name));
                    ClearErrors(nameof(Name));
                    OnErrorChanged(nameof(Name));

                    await budgetOverviewViewModel.Refresh();
                })
                .AddTo(_compositeDisposable);

            DeselectParentCommand = new RxRelayCommand(() => Parent.Value = null);
            
            Parent = new ReactiveProperty<ICategoryViewModel>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .SetValidateNotifyError(parent => ValidateNewCategoryRelationParent(Name, parent))
                .AddTo(_compositeDisposable);

            IsIncomeRelevant = new ReactiveProperty<bool>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(_compositeDisposable);

            IsIncomeRelevant.Subscribe(_ =>
            {
                (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                ValidateName();
            }).AddTo(_compositeDisposable);
       
            MonthOffset = new ReactiveProperty<int>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(_compositeDisposable);

            this.ObservePropertyChanges(nameof(Name))
                .Subscribe(_ => (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate())
                .AddTo(_compositeDisposable);
            Parent
                .Subscribe(_ => ValidateName())
                .AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
                ValidateName();
            }
        }

        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Parent { get; }

        public IReactiveProperty<bool> IsIncomeRelevant { get; }
        public IReactiveProperty<int> MonthOffset { get; }

        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ICommand AddCommand { get; }

        public ICommand DeselectParentCommand { get; }

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

        private bool ValidateName()
        {
            bool ret;
            if (string.IsNullOrWhiteSpace(Name))
            {
                SetErrors(_localizer.Localize("ErrorMessageCategoryNameEmpty").ToEnumerable(), nameof(Name));
                ret = false;
            }
            else if (!ValidateNewCategoryRelationCondition())
            {
                SetErrors(_localizer.Localize("ErrorMessageWrongCategoryName").ToEnumerable(), nameof(Name));
                ret = false;
            }
            else
            {
                ClearErrors(nameof(Name));
                ret = true;
            }
            OnErrorChanged(nameof(Name));
            return ret;

            bool ValidateNewCategoryRelationCondition()
            {
                if (IsIncomeRelevant?.Value ?? false)
                    return _incomeCategoryViewModelService.All.All(icvm => icvm.Name != Name);
                return Parent.Value is null && AllPotentialParents.Where(cvw => cvw.Parent is null).All(cvm => cvm.Name != Name) ||
                       Parent.Value != null && Parent.Value.Categories.All(cvm => cvm != null && cvm.Name != Name);
            }
        }
    }
}
