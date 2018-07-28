using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.MVVM.ViewModels.ForModels.Utility;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;


namespace BFF.MVVM.ViewModels
{
    public interface IBudgetMonthViewModel
    {
        ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        DateTime Month { get; }
        long NotBudgetedInPreviousMonth { get; }
        long OverspentInPreviousMonth { get; }
        long IncomeForThisMonth { get; }
        long DanglingTransferForThisMonth { get; }
        long UnassignedTransactionSumForThisMonth { get; }
        long BudgetedThisMonth { get; }
        long BudgetedThisMonthPositive { get; }
        long AvailableToBudget { get; }
        long Outflows { get; }
        long Balance { get; }

        ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }
    }

    public sealed class BudgetMonthViewModel : ViewModelBase, IBudgetMonthViewModel, IDisposable, ITransientViewModel
    {
        private readonly IBudgetMonth _budgetMonth;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BudgetMonthViewModel(
            IBudgetMonth budgetMonth,
            ITransRepository transRepository,
            Func<Func<Task<IEnumerable<ITransLikeViewModel>>>, ILazyTransLikeViewModels> lazyTransLikeViewModelsFactory,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IBudgetEntryViewModelService budgetEntryViewModelService)
        {
            _budgetMonth = budgetMonth;
            BudgetEntries = budgetMonth.BudgetEntries.ToReadOnlyReactiveCollection(budgetEntryViewModelService.GetViewModel).AddTo(_compositeDisposable);

            budgetMonth
                .ObservePropertyChanges(bm => bm.Month)
                .Subscribe(fa => OnPropertyChanged(nameof(Month)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.NotBudgetedInPreviousMonth)
                .Subscribe(fa => OnPropertyChanged(nameof(NotBudgetedInPreviousMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.OverspentInPreviousMonth)
                .Subscribe(fa => OnPropertyChanged(nameof(OverspentInPreviousMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.IncomeForThisMonth)
                .Subscribe(fa => OnPropertyChanged(nameof(IncomeForThisMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.DanglingTransferForThisMonth)
                .Subscribe(fa => OnPropertyChanged(nameof(DanglingTransferForThisMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.UnassignedTransactionSumForThisMonth)
                .Subscribe(fa => OnPropertyChanged(nameof(UnassignedTransactionSumForThisMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.BudgetedThisMonth)
                .Subscribe(fa =>
                {
                    OnPropertyChanged(nameof(BudgetedThisMonth));
                    OnPropertyChanged(nameof(BudgetedThisMonthPositive));
                })
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.Outflows)
                .Subscribe(fa => OnPropertyChanged(nameof(Outflows)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.Balance)
                .Subscribe(fa => OnPropertyChanged(nameof(Balance)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(bm => bm.AvailableToBudget)
                .Subscribe(fa => OnPropertyChanged(nameof(AvailableToBudget)))
                .AddTo(_compositeDisposable);

            AssociatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                    convertFromTransBaseToTransLikeViewModel.Convert(
                        (await transRepository.GetFromMontAsync(Month).ConfigureAwait(false))
                        .Where(tb => tb as IHaveCategory is null || tb is IHaveCategory hc && hc.Category as IIncomeCategory is null), 
                        null))
                .AddHere(_compositeDisposable);
        }

        public ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        public DateTime Month => _budgetMonth.Month;
        public long NotBudgetedInPreviousMonth => _budgetMonth.NotBudgetedInPreviousMonth;
        public long OverspentInPreviousMonth => _budgetMonth.OverspentInPreviousMonth;
        public long IncomeForThisMonth => _budgetMonth.IncomeForThisMonth;
        public long DanglingTransferForThisMonth => _budgetMonth.DanglingTransferForThisMonth;
        public long UnassignedTransactionSumForThisMonth => _budgetMonth.UnassignedTransactionSumForThisMonth;
        public long BudgetedThisMonth => _budgetMonth.BudgetedThisMonth;
        public long BudgetedThisMonthPositive => BudgetedThisMonth * -1;

        public long AvailableToBudget => _budgetMonth.AvailableToBudget;

        public long Outflows => _budgetMonth.Outflows;
        public long Balance => _budgetMonth.Balance;
        public ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
