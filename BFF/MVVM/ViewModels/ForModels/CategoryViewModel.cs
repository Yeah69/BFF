using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using MoreLinq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
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

    public class CategoryViewModel : CategoryBaseViewModel, ICategoryViewModel
    {
        private readonly ICategory _category;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly Lazy<IBudgetOverviewViewModel> _budgetOverviewViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public class CategoryViewModelInitializer : ICategoryViewModelInitializer
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
                    viewModel.Categories =
                        viewModel._category.Categories.ToReadOnlyReactiveCollection(_service.Value.GetViewModel).AddTo(viewModel.CompositeDisposable);

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
        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; private set; }

        /// <summary>
        /// The Parent
        /// </summary>
        public ICategoryViewModel Parent { get; private set; }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    string.Format("ConfirmationDialog_ConfirmCategoryDeletion".Localize(), Name),
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
                    "ConfirmationDialog_Title".Localize(),
                    string.Format("ConfirmationDialog_ConfirmCategoryMerge".Localize(), Name, target.Name),
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
                   && categoryViewModel._category.Id != _category.Id
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
            ISummaryAccountViewModel summaryAccountViewModel,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IAccountViewModelService accountViewModelService,
            Lazy<IBudgetOverviewViewModel> budgetOverviewViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(category, rxSchedulerProvider)
        {
            _category = category;
            _summaryAccountViewModel = summaryAccountViewModel;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _accountViewModelService = accountViewModelService;
            _budgetOverviewViewModel = budgetOverviewViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;
        }
    }
}