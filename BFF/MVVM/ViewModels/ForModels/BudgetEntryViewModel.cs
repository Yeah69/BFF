using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IBudgetEntryViewModel : IDataModelViewModel
    {
        /// <summary>
        /// Each SubTransaction can be budgeted to a category.
        /// </summary>
        ICategoryViewModel Category { get; }

        DateTime Month { get; }
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }
    }

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        private readonly IBudgetEntry _budgetEntry;
        public ICategoryViewModel Category { get; private set; }
        public DateTime Month => _budgetEntry.Month;
        public long Budget
        {
            get => _budgetEntry.Budget;
            set => _budgetEntry.Budget = value;
        }
        public long Outflow => _budgetEntry.Outflow;
        public long Balance => _budgetEntry.Balance;

        public BudgetEntryViewModel(
            IBudgetEntry budgetEntry,
            Lazy<IBudgetOverviewViewModel> budgetOverviewViewModel,
            ICategoryViewModelService categoryViewModelService,
            IRxSchedulerProvider rxSchedulerProvider) : base(budgetEntry, rxSchedulerProvider)
        {
            _budgetEntry = budgetEntry;
            Category = categoryViewModelService.GetViewModel(budgetEntry.Category);
            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Category))
                .Do(_ => Category = categoryViewModelService.GetViewModel(budgetEntry.Category))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Category)))
                .AddTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Month))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Month)))
                .AddTo(CompositeDisposable);

            var observeBudgetChanges = budgetEntry.ObservePropertyChanges(nameof(IBudgetEntry.Budget));
            observeBudgetChanges
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Budget)))
                .AddTo(CompositeDisposable);

            observeBudgetChanges
                .Subscribe(async _ => await Task.Factory.StartNew(budgetOverviewViewModel.Value.Refresh))
                .AddTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Outflow))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Outflow)))
                .AddTo(CompositeDisposable);
            
            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Balance))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Balance)))
                .AddTo(CompositeDisposable);
        }
    }
}
