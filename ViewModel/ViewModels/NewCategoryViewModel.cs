using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface INewCategoryViewModel
    {
        string? Name { get; set; }

        IReactiveProperty<ICategoryViewModel?> Parent { get; }

        IReactiveProperty<bool> IsIncomeRelevant { get; }

        IReactiveProperty<int> MonthOffset { get; }

        ICommand AddCommand { get; }

        ICommand DeselectParentCommand { get; }

        IObservableReadOnlyList<ICategoryViewModel>? AllPotentialParents { get; }

        IObservableReadOnlyList<ICategoryBaseViewModel> All { get; }

        IHaveCategoryViewModel? CurrentCategoryOwner { get; set; }
    }

    internal sealed class NewCategoryViewModel : NotifyingErrorViewModelBase, INewCategoryViewModel, IOncePerBackend, IDisposable
    {
        private readonly ICategoryViewModelService _categoryViewModelService;
        private readonly IIncomeCategoryViewModelService _incomeCategoryViewModelService;
        private readonly ICategoryBaseViewModelService _categoryBaseViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new();
        private string? _name;

        public NewCategoryViewModel(
            ICreateNewModels createNewModels,
            ICategoryViewModelInitializer categoryViewModelInitializer,
            ICategoryViewModelService categoryViewModelService,
            IIncomeCategoryViewModelService incomeCategoryViewModelService,
            ICategoryBaseViewModelService categoryBaseViewModelService)
        {
            bool ValidateNewCategoryRelationCondition(string? text, ICategoryViewModel? parent)
            {
                if(IsIncomeRelevant.Value)
                    return incomeCategoryViewModelService.All?.All(icvm => icvm.Name != text) ?? true;
                return parent is null && (AllPotentialParents?.Where(cvw => cvw.Parent is null).All(cvm => cvm.Name != text) ?? true) ||
                       parent is not null && parent.Categories.All(cvm => cvm is not null && cvm.Name != text);
            }
            string? ValidateNewCategoryRelationParent(string? text, ICategoryViewModel? parent)
            {
                return ValidateNewCategoryRelationCondition(text, parent)
                    ? null
                    : ""; // ToDo _localizer.Localize("ErrorMessageWrongCategoryParent");
            }

            _categoryViewModelService = categoryViewModelService;
            _incomeCategoryViewModelService = incomeCategoryViewModelService;
            _categoryBaseViewModelService = categoryBaseViewModelService;

            IsIncomeRelevant = new ReactiveProperty<bool>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(_compositeDisposable);
            
            Parent = new ReactiveProperty<ICategoryViewModel?>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .SetValidateNotifyError(parent => ValidateNewCategoryRelationParent(Name, parent))
                .AddTo(_compositeDisposable);
       
            MonthOffset = new ReactiveProperty<int>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(_compositeDisposable);

            AddCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    _compositeDisposable,
                    async () =>
                    {
                        (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                        if (Parent.HasErrors || !ValidateName()) return;

                        if (IsIncomeRelevant.Value)
                        {
                            IIncomeCategory newCategory = createNewModels.CreateIncomeCategory();
                            newCategory.Name = Name?.Trim() ?? String.Empty;
                            newCategory.MonthOffset = MonthOffset.Value;
                            await newCategory.InsertAsync();
                            if (CurrentCategoryOwner is not null)
                                CurrentCategoryOwner.Category =
                                    incomeCategoryViewModelService.GetViewModel(newCategory);
                            CurrentCategoryOwner = null;
                        }
                        else
                        {
                            ICategory newCategory = createNewModels.CreateCategory();
                            newCategory.Parent = _categoryViewModelService.GetModel(Parent.Value);
                            newCategory.Name = Name?.Trim() ?? String.Empty;
                            newCategory.Parent?.AddCategory(newCategory);
                            await newCategory.InsertAsync();
                            OnPropertyChanged(nameof(AllPotentialParents));
                            var categoryViewModel = _categoryViewModelService.GetViewModel(newCategory) ??
                                                    throw new NullReferenceException("Shouldn't be null");
                            categoryViewModelInitializer.Initialize(categoryViewModel);
                            if (CurrentCategoryOwner is not null)
                                CurrentCategoryOwner.Category = categoryViewModel;
                            CurrentCategoryOwner = null;
                        }

                        _name = "";
                        OnPropertyChanged(nameof(Name));
                        ClearErrors(nameof(Name));
                        OnErrorChanged(nameof(Name));
                    });

            DeselectParentCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => Parent.Value = null);

            IsIncomeRelevant.Subscribe(_ =>
            {
                (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate();
                ValidateName();
            }).AddTo(_compositeDisposable);

            this.ObservePropertyChanged(nameof(Name))
                .Subscribe(_ => (Parent as ReactiveProperty<ICategoryViewModel>)?.ForceValidate())
                .AddTo(_compositeDisposable);
            Parent
                .Subscribe(_ => ValidateName())
                .AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string? Name
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
        public IReactiveProperty<ICategoryViewModel?> Parent { get; }

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
        public IObservableReadOnlyList<ICategoryViewModel>? AllPotentialParents => _categoryViewModelService.All;

        public IObservableReadOnlyList<ICategoryBaseViewModel> All => _categoryBaseViewModelService.All;

        public IHaveCategoryViewModel? CurrentCategoryOwner { get; set; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private bool ValidateName()
        {
            bool ret;
            if (string.IsNullOrWhiteSpace(Name))
            {
                SetErrors("" // ToDo _localizer.Localize("ErrorMessageCategoryNameEmpty")
                    .ToEnumerable(), nameof(Name));
                ret = false;
            }
            else if (!ValidateNewCategoryRelationCondition())
            {
                SetErrors("" // ToDo _localizer.Localize("ErrorMessageWrongCategoryName")
                    .ToEnumerable(), nameof(Name));
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
                if (IsIncomeRelevant.Value)
                    return _incomeCategoryViewModelService.All?.All(icvm => icvm.Name != Name) ?? true;
                return Parent.Value is null && (AllPotentialParents?.Where(cvw => cvw.Parent is null).All(cvm => cvm.Name != Name) ?? true) ||
                       Parent.Value is not null && Parent.Value.Categories.All(cvm => cvm is not null && cvm.Name != Name);
            }
        }
    }
}
