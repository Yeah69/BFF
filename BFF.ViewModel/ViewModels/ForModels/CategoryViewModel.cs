using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using MoreLinq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface ICategoryViewModel : ICategoryBaseViewModel
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        /// <summary>
        /// The Parent
        /// </summary>
        ICategoryViewModel Parent { get; }

        void MergeTo(ICategoryViewModel target);

        bool CanMergeTo(ICategoryViewModel target);
    }

    public interface ICategoryViewModelInitializer
    {
        void Initialize(IEnumerable<ICategoryViewModel> categoryViewModels);
        void Initialize(ICategoryViewModel categoryViewModel);
    }

    internal class CategoryViewModel : CategoryBaseViewModel, ICategoryViewModel
    {
        private readonly ICategory _category;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly ILocalizer _localizer;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly Lazy<IBudgetOverviewViewModel> _budgetOverviewViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        internal class CategoryViewModelInitializer : ICategoryViewModelInitializer
        {
            private readonly Lazy<ICategoryViewModelService> _service;
            private readonly IRxSchedulerProvider _rxSchedulerProvider;

            public CategoryViewModelInitializer(
                Lazy<ICategoryViewModelService> service,
                IRxSchedulerProvider rxSchedulerProvider)
            {
                _service = service;
                _rxSchedulerProvider = rxSchedulerProvider;
            }

            public void Initialize(IEnumerable<ICategoryViewModel> categoryViewModels)
            {
                foreach (var categoryViewModel in categoryViewModels)
                {
                    Initialize(categoryViewModel);
                }
            }

            public void Initialize(ICategoryViewModel categoryViewModel)
            {
                if (categoryViewModel is CategoryViewModel viewModel)
                {
                    viewModel.Parent = _service.Value.GetViewModel(viewModel._category.Parent);
                    viewModel._category
                        .ObservePropertyChanges(nameof(viewModel._category.Parent))
                        .ObserveOn(_rxSchedulerProvider.UI)
                        .Subscribe(_ =>
                        {
                            viewModel.Parent = _service.Value.GetViewModel(viewModel._category.Parent);
                            viewModel.OnPropertyChanged(nameof(viewModel.Parent));
                        })
                        .AddTo(viewModel.CompositeDisposable);
                }
            }
        }

        /// <summary>
        /// The Child-Categories
        /// </summary>
        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        /// <summary>
        /// The Parent
        /// </summary>
        public ICategoryViewModel Parent { get; private set; }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    string.Format(_localizer.Localize("ConfirmationDialog_ConfirmCategoryDeletion"), Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(async r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        await base.DeleteAsync();
                        _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                        _summaryAccountViewModel.RefreshTransCollection();
                    }

                    await _budgetOverviewViewModel.Value.Refresh();

                    source.SetResult(Unit.Default);
                });
            return source.Task;
        }

        public void MergeTo(ICategoryViewModel target)
        {
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    string.Format(_localizer.Localize("ConfirmationDialog_ConfirmCategoryMerge"), Name, target.Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(r =>
                {
                    if (r != BffMessageDialogResult.Affirmative) return;

                    if (target is CategoryViewModel categoryViewModel)
                    {
                        Observable
                            .FromAsync(token => _category.MergeToAsync(categoryViewModel._category), _rxSchedulerProvider.Task)
                            .ObserveOn(_rxSchedulerProvider.UI)
                            .Subscribe(_ =>
                            {
                                _summaryAccountViewModel.RefreshTransCollection();
                                _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                            });
                    }
                });
        }

        public bool CanMergeTo(ICategoryViewModel target)
        {
            return target is CategoryViewModel categoryViewModel
                   && categoryViewModel._category != _category
                   && categoryViewModel._category.Name != _category.Name
                   && !categoryViewModel._category.IsMyAncestor(_category);
        }

        public override string FullName => $"{(Parent != null ? $"{Parent.FullName}." : "")}{Name}";

        public override IEnumerable<ICategoryBaseViewModel> FullChainOfCategories
        {
            get
            {
                ICategoryViewModel current = this;
                IList<ICategoryBaseViewModel> temp = new List<ICategoryBaseViewModel> { current };
                while (current.Parent != null)
                {
                    current = current.Parent;
                    temp.Add(current);
                }
                return temp.Reverse();
            }
        }

        public override int Depth => Parent?.Depth + 1 ?? 0;

        public override string GetIndent()
        {
            return $"{Parent?.GetIndent()}. ";
        }

        public CategoryViewModel(
            ICategory category,
            Lazy<ICategoryViewModelService> service,
            ISummaryAccountViewModel summaryAccountViewModel,
            ILocalizer localizer,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IAccountViewModelService accountViewModelService,
            Lazy<IBudgetOverviewViewModel> budgetOverviewViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(category, rxSchedulerProvider)
        {
            _category = category;
            Categories =
                _category.Categories.ToReadOnlyReactiveCollection(service.Value.GetViewModel).AddTo(CompositeDisposable);
            _category.ObservePropertyChanges(nameof(_category.Parent))
                .Select(_ => Parent = service.Value.GetViewModel(_category.Parent))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Parent)));
            _summaryAccountViewModel = summaryAccountViewModel;
            _localizer = localizer;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _accountViewModelService = accountViewModelService;
            _budgetOverviewViewModel = budgetOverviewViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;
        }
    }
}