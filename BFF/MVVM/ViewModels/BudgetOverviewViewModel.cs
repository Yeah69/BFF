using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

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
        private readonly ICategoryViewModelService _categoryViewModelService;
        private double _verticalOffset;
        public IList<IBudgetMonthViewModel> BudgetMonths { get; }

        public IObservableReadOnlyList<ICategoryViewModel> Categories => _categoryViewModelService.All;

        public IBudgetMonthViewModel SelectedBudgetMonth { get; private set; }

        public double VerticalOffset
        {
            get => _verticalOffset;
            set
            {
                _verticalOffset = value;
                this.OnPropertyChanged();
            }
        }

        public BudgetOverviewViewModel(IBudgetMonthRepository budgetMonthRepository, IBudgetEntryViewModelService budgetEntryViewModelService, ICategoryViewModelService categoryViewModelService)
        {
            _budgetMonthRepository = budgetMonthRepository;
            _budgetEntryViewModelService = budgetEntryViewModelService;
            _categoryViewModelService = categoryViewModelService;
            BudgetMonths = CreateBudgetMonths();
            SelectedBudgetMonth = BudgetMonths[monthToIndex(DateTime.Now)];
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
