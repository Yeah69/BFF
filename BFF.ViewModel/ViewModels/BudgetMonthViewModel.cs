using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetMonthViewModel
    {
        DateTime Month { get; }
        string MonthName { get; }
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
        ILazyTransLikeViewModels AssociatedIncomeTransElementsViewModel { get; }

        IRxRelayCommand EmptyCellsBudgetLastMonth { get; }

        IRxRelayCommand EmptyCellsOutflowsLastMonth { get; }

        IRxRelayCommand EmptyCellsAvgOutflowsLastThreeMonths { get; }

        IRxRelayCommand EmptyCellsAvgOutflowsLastYear { get; }

        IRxRelayCommand EmptyCellsBalanceToZero { get; }

        IRxRelayCommand AllCellsZero { get; }
    }
    
    

    public sealed class BudgetMonthViewModel : ViewModelBase, IBudgetMonthViewModel, IDisposable, ITransient
    {
        private readonly IBudgetMonth _budgetMonth;
        private readonly IBffSettings _bffSettings;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BudgetMonthViewModel(
            IBudgetMonth budgetMonth,
            IBffSettings bffSettings,
            IBudgetRefreshes budgetRefreshes,
            Func<Func<Task<IEnumerable<ITransLikeViewModel>>>, ILazyTransLikeViewModels> lazyTransLikeViewModelsFactory,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel)
        {
            _budgetMonth = budgetMonth;
            _bffSettings = bffSettings;

            AssociatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                    convertFromTransBaseToTransLikeViewModel.Convert(
                        (await budgetMonth.GetAssociatedTransAsync().ConfigureAwait(false))
                        .Where(tb => !(tb is IHaveCategory) || tb is IHaveCategory hc && !(hc.Category is IIncomeCategory)), 
                        null))
                .AddForDisposalTo(_compositeDisposable);

            AssociatedIncomeTransElementsViewModel = lazyTransLikeViewModelsFactory(async () => 
                    convertFromTransBaseToTransLikeViewModel.Convert(
                        await budgetMonth.GetAssociatedTransForIncomeCategoriesAsync(),
                        null))
                .AddForDisposalTo(_compositeDisposable);

            EmptyCellsBudgetLastMonth = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgBudget(1);
                    budgetRefreshes.RefreshCompletely();
                })
                .AddForDisposalTo(_compositeDisposable);

            EmptyCellsOutflowsLastMonth = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgOutflow(1);
                    budgetRefreshes.RefreshCompletely();
                })
                .AddForDisposalTo(_compositeDisposable);

            EmptyCellsAvgOutflowsLastThreeMonths = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgOutflow(3);
                    budgetRefreshes.RefreshCompletely();
                })
                .AddForDisposalTo(_compositeDisposable);

            EmptyCellsAvgOutflowsLastYear = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgOutflow(12);
                    budgetRefreshes.RefreshCompletely();
                })
                .AddForDisposalTo(_compositeDisposable);

            EmptyCellsBalanceToZero = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToBalanceZero();
                    budgetRefreshes.RefreshCompletely();
                })
                .AddForDisposalTo(_compositeDisposable);

            AllCellsZero = new RxRelayCommand(async () =>
                {
                    await budgetMonth.AllBudgetEntriesToZero();
                    budgetRefreshes.RefreshCompletely();
                })
                .AddForDisposalTo(_compositeDisposable);
        }
        
        public DateTime Month => _budgetMonth.Month;

        public string MonthName =>
            _bffSettings.Culture_DefaultDateLong
                ? Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetMonthName(this.Month.Month)
                : Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(this.Month.Month);
        
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
        public ILazyTransLikeViewModels AssociatedIncomeTransElementsViewModel { get; }
        public IRxRelayCommand EmptyCellsBudgetLastMonth { get; }
        public IRxRelayCommand EmptyCellsOutflowsLastMonth { get; }
        public IRxRelayCommand EmptyCellsAvgOutflowsLastThreeMonths { get; }
        public IRxRelayCommand EmptyCellsAvgOutflowsLastYear { get; }
        public IRxRelayCommand EmptyCellsBalanceToZero { get; }
        public IRxRelayCommand AllCellsZero { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
