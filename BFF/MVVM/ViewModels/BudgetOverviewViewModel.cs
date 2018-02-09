using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        IList<IBudgetMonthViewModel> BudgetMonths { get; }

        IReactiveProperty<int> CurrentMonthStartIndex { get; }

        IReactiveProperty<bool> IsOpen { get; }
        int SelectedIndex { get; set; }

        ReactiveCommand IncreaseMonthStartIndex { get; }

        ReactiveCommand DecreaseMonthStartIndex { get; }
    }

    public class BudgetOverviewViewModel : ViewModelBase, IBudgetOverviewViewModel, IDisposable
    {
        private static readonly int LastMonthIndex = MonthToIndex(DateTime.MaxValue);

        private readonly IBudgetMonthRepository _budgetMonthRepository;
        private readonly IBudgetEntryViewModelService _budgetEntryViewModelService;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _selectedIndex;
        public IList<IBudgetMonthViewModel> BudgetMonths { get; private set; }

        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        public IBudgetMonthViewModel SelectedBudgetMonth { get; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public IReactiveProperty<int> CurrentMonthStartIndex { get; }

        public IReactiveProperty<bool> IsOpen { get; }

        public ReactiveCommand IncreaseMonthStartIndex { get; }

        public ReactiveCommand DecreaseMonthStartIndex { get; }

        public BudgetOverviewViewModel(
            IBudgetMonthRepository budgetMonthRepository,
            IBudgetEntryViewModelService budgetEntryViewModelService,
            ICategoryViewModelService categoryViewModelService,
            ICategoryRepository categoryRepository)
        {
            _budgetMonthRepository = budgetMonthRepository;
            _budgetEntryViewModelService = budgetEntryViewModelService;

            SelectedIndex = -1;

            Categories =
                categoryRepository
                    .All
                    .ToReadOnlyReactiveCollection(categoryViewModelService.GetViewModel);

            BudgetMonths = CreateBudgetMonths();
            int index = MonthToIndex(DateTime.Now) - 7;
            SelectedBudgetMonth = BudgetMonths[index];
            CurrentMonthStartIndex = new ReactiveProperty<int>(index);

            IncreaseMonthStartIndex = CurrentMonthStartIndex.Select(i => i < LastMonthIndex - 1).ToReactiveCommand()
                .AddTo(_compositeDisposable);
            IncreaseMonthStartIndex.Subscribe(_ => CurrentMonthStartIndex.Value = CurrentMonthStartIndex.Value + 1)
                .AddTo(_compositeDisposable);
            DecreaseMonthStartIndex = CurrentMonthStartIndex.Select(i => i > 0).ToReactiveCommand()
                .AddTo(_compositeDisposable);
            DecreaseMonthStartIndex.Subscribe(_ => CurrentMonthStartIndex.Value = CurrentMonthStartIndex.Value - 1)
                .AddTo(_compositeDisposable);
            IsOpen =
                new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged)
                    .AddTo(_compositeDisposable);
            IsOpen.Where(b => b).Subscribe(b => Refresh()).AddTo(_compositeDisposable);

            Messenger.Default.Register<CultureMessage>(this, message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                        break;
                    case CultureMessage.RefreshCurrency:
                    case CultureMessage.RefreshDate:
                        Refresh();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            });

            Messenger.Default.Register<BudgetOverviewMessage>(this, message =>
            {
                switch (message)
                {
                    case BudgetOverviewMessage.Refresh:
                        Refresh();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            });

            Disposable.Create(() =>
            {
                Messenger.Default.Unregister<CultureMessage>(this);
                Messenger.Default.Unregister<BudgetOverviewMessage>(this);
            }).AddTo(_compositeDisposable);
        }

        private void Refresh()
        {
            BudgetMonths = CreateBudgetMonths();
            OnPropertyChanged(nameof(BudgetMonths));
        }

        private IDataVirtualizingCollection<IBudgetMonthViewModel> CreateBudgetMonths()
        {
            return CollectionBuilder<IBudgetMonthViewModel>
                .CreateBuilder()
                .BuildAHoardingPreloadingSyncCollection(
                    new RelayBasicSyncDataAccess<IBudgetMonthViewModel>(
                        (offset, pageSize) =>
                        {
                            DateTime fromMonth = IndexToMonth(offset);
                            DateTime toMonth = IndexToMonth(offset + pageSize - 1);

                            return _budgetMonthRepository.Find(fromMonth, toMonth)
                                .Select(bm =>
                                    (IBudgetMonthViewModel) new BudgetMonthViewModel(bm, _budgetEntryViewModelService))
                                .ToArray();
                        },
                        () => LastMonthIndex),
                    6);
        }

        private static DateTime IndexToMonth(int index)
        {
            int year = index / 12 + 1;
            int month = index % 12 + 1;

            return new DateTime(year, month, 1);
        }

        private static int MonthToIndex(DateTime month)
        {
            return (month.Year - 1) * 12 + month.Month - 1;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}