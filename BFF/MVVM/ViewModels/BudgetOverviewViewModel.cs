using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using Reactive.Bindings;
using Reactive.Bindings.Helpers;

namespace BFF.MVVM.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        IList<IBudgetMonthViewModel> BudgetMonths { get; }
    }

    public class BudgetOverviewViewModel : ObservableObject, IBudgetOverviewViewModel
    {
        private readonly IBudgetMonthRepository _budgetMonthRepository;
        private readonly IBudgetEntryViewModelService _budgetEntryViewModelService;
        private double _verticalOffset;
        private int _selectedIndex;
        private int _currentMonthStartIndex;
        public IList<IBudgetMonthViewModel> BudgetMonths { get; }

        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        public IBudgetMonthViewModel SelectedBudgetMonth { get; }

        public double VerticalOffset
        {
            get => _verticalOffset;
            set
            {
                if (Math.Abs(_verticalOffset - value) < 0.001) return;
                _verticalOffset = value;
                OnPropertyChanged();
            }
        }

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

        public int CurrentMonthStartIndex
        {
            get => _currentMonthStartIndex;
            set
            {
                if (_currentMonthStartIndex == value) return;
                _currentMonthStartIndex = value;
                OnPropertyChanged();
            }
        }

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
                    .ToFilteredReadOnlyObservableCollection(
                        c => c.Name != "Available this month" // TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! 
                        && c.Name != "Available next month")// TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! 
                    .ToReadOnlyReactiveCollection(categoryViewModelService.GetViewModel);

            BudgetMonths = CreateBudgetMonths();
            int index = monthToIndex(DateTime.Now);
            SelectedBudgetMonth = BudgetMonths[index];
            CurrentMonthStartIndex = index;
        }

        private IDataVirtualizingCollection<IBudgetMonthViewModel> CreateBudgetMonths()
        {
            return CollectionBuilder<IBudgetMonthViewModel>
                .CreateBuilder()
                .BuildAHoardingPreloadingSyncCollection(
                    new RelayBasicSyncDataAccess<IBudgetMonthViewModel>(
                        (offset, pageSize) =>
                        {
                            DateTime fromMonth = indexToMonth(offset);
                            DateTime toMonth = indexToMonth(offset + pageSize - 1);

                            return _budgetMonthRepository.Find(fromMonth, toMonth, null)
                                .Select(bm => (IBudgetMonthViewModel) new BudgetMonthViewModel(bm, _budgetEntryViewModelService))
                                .ToArray();
                        },
                        () => 119988),
                    6);
        }

        private DateTime indexToMonth(int index)
        {
            int year = index / 12 + 1;
            int month = index % 12 + 1;

            return new DateTime(year, month, 1);
        }

        private int monthToIndex(DateTime month)
        {
            return (month.Year - 1) * 12 + month.Month - 1;
        }
    }
}
