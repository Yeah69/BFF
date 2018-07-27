using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
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

        long AggregatedBudget { get; }
        long AggregatedOutflow { get; }
        long AggregatedBalance { get; }

        IReadOnlyList<IBudgetEntryViewModel> Children { get; set; }

        IEnumerable<ITransLikeViewModel> AssociatedTransElements { get; }

        bool OpenAssociatedTransPopupFlag { get; set; }

        IRxRelayCommand OpenAssociatedTransElements { get; }

        IRxRelayCommand CloseAssociatedTransElements { get; }
    }

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        public ITransDataGridColumnManager TransDataGridColumnManager { get; }
        private readonly IBudgetEntry _budgetEntry;
        private IReadOnlyList<IBudgetEntryViewModel> _children;
        private bool _openAssociatedTransPopupFlag;

        public ICategoryViewModel Category { get; private set; }
        public DateTime Month => _budgetEntry.Month;
        public long Budget
        {
            get => _budgetEntry.Budget;
            set => _budgetEntry.Budget = value;
        }
        public long Outflow => _budgetEntry.Outflow;
        public long Balance => _budgetEntry.Balance;
        public long AggregatedBudget { get; private set; }
        public long AggregatedOutflow { get; private set; }
        public long AggregatedBalance { get; private set; }

        public IReadOnlyList<IBudgetEntryViewModel> Children
        {
            get => _children;
            set
            {
                if (Equals(_children, value)) return; 
                _children = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ITransLikeViewModel> AssociatedTransElements { get; private set; }

        public bool OpenAssociatedTransPopupFlag
        {
            get => _openAssociatedTransPopupFlag;
            set
            {
                if (value == _openAssociatedTransPopupFlag) return;
                _openAssociatedTransPopupFlag = value;
                OnPropertyChanged();
            }
        }

        public IRxRelayCommand OpenAssociatedTransElements { get; }
        public IRxRelayCommand CloseAssociatedTransElements { get; }

        public BudgetEntryViewModel(
            IBudgetEntry budgetEntry,
            Lazy<IBudgetOverviewViewModel> budgetOverviewViewModel,
            ITransRepository transRepository,
            ITransDataGridColumnManager transDataGridColumnManager,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            ICategoryViewModelService categoryViewModelService,
            IRxSchedulerProvider rxSchedulerProvider) : base(budgetEntry, rxSchedulerProvider)
        {
            TransDataGridColumnManager = transDataGridColumnManager;
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

            Children = new List<IBudgetEntryViewModel>();

            AggregatedBudget = Budget + Children.Sum(bevm => bevm.AggregatedBudget);
            AggregatedBalance = Balance + Children.Sum(bevm => bevm.AggregatedBalance);
            AggregatedOutflow = Outflow + Children.Sum(bevm => bevm.AggregatedOutflow);

            var updatesFromChildren = new SerialDisposable().AddHere(CompositeDisposable);

            var updateAggregates = Observer.Create<Unit>(_ =>
            {
                AggregatedBudget = Budget + Children.Sum(bevm => bevm.AggregatedBudget);
                AggregatedBalance = Balance + Children.Sum(bevm => bevm.AggregatedBalance);
                AggregatedOutflow = Outflow + Children.Sum(bevm => bevm.AggregatedOutflow);
                OnPropertyChanged(nameof(AggregatedBudget));
                OnPropertyChanged(nameof(AggregatedBalance));
                OnPropertyChanged(nameof(AggregatedOutflow));
            });

            this.ObservePropertyChanges(nameof(Children))
                .Do(_ =>
                    {
                        updatesFromChildren.Disposable = Observable.Merge(Children
                            .Select(bevm =>
                                bevm.ObservePropertyChanges(nameof(AggregatedBudget)))
                            .Concat(Children
                                .Select(bevm =>
                                    bevm.ObservePropertyChanges(nameof(AggregatedOutflow))))
                            .Concat(Children
                                .Select(bevm =>
                                    bevm.ObservePropertyChanges(nameof(AggregatedBalance))))).Subscribe(updateAggregates);
                    })
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(updateAggregates)
                .AddTo(CompositeDisposable);

            AssociatedTransElements = Enumerable.Empty<ITransLikeViewModel>();

            OpenAssociatedTransElements = new AsyncRxRelayCommand(async () =>
                    {
                        AssociatedTransElements = convertFromTransBaseToTransLikeViewModel.Convert(
                            await transRepository.GetFromMontAndCategoryAsync(Month,
                                categoryViewModelService.GetModel(Category)), null);
                        OnPropertyChanged(nameof(AssociatedTransElements));
                        if (AssociatedTransElements.Any().Not())
                            OpenAssociatedTransPopupFlag = false;
                    })
                .AddHere(CompositeDisposable);

            CloseAssociatedTransElements = new RxRelayCommand(() =>
                    {
                        AssociatedTransElements = Enumerable.Empty<ITransLikeViewModel>();
                        OnPropertyChanged(nameof(AssociatedTransElements));
                    })
                .AddHere(CompositeDisposable);
        }
    }
}
