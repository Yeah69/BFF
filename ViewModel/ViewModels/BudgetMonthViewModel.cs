using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Reactive.Extensions;

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

        ILazyTransLikeViewModels? AssociatedTransElementsViewModel { get; }
        ILazyTransLikeViewModels? AssociatedIncomeTransElementsViewModel { get; }

        ICommand EmptyCellsBudgetLastMonth { get; }

        ICommand EmptyCellsOutflowsLastMonth { get; }

        ICommand EmptyCellsAvgOutflowsLastThreeMonths { get; }

        ICommand EmptyCellsAvgOutflowsLastYear { get; }

        ICommand EmptyCellsBalanceToZero { get; }

        ICommand AllCellsZero { get; }
    }
    
    

    public sealed class BudgetMonthViewModel : ViewModelBase, IBudgetMonthViewModel, IDisposable, ITransient
    {
        private readonly IBudgetMonth _budgetMonth;
        private readonly IBffSettings _bffSettings;
        private readonly CompositeDisposable _compositeDisposable = new();

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
                .CompositeDisposalWith(_compositeDisposable);

            AssociatedIncomeTransElementsViewModel = lazyTransLikeViewModelsFactory(async () => 
                    convertFromTransBaseToTransLikeViewModel.Convert(
                        await budgetMonth.GetAssociatedTransForIncomeCategoriesAsync(),
                        null))
                .CompositeDisposalWith(_compositeDisposable);

            EmptyCellsBudgetLastMonth = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgBudget(1);
                    budgetRefreshes.RefreshCompletely();
                })
                .CompositeDisposalWith(_compositeDisposable);

            EmptyCellsOutflowsLastMonth = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgOutflow(1);
                    budgetRefreshes.RefreshCompletely();
                })
                .CompositeDisposalWith(_compositeDisposable);

            EmptyCellsAvgOutflowsLastThreeMonths = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgOutflow(3);
                    budgetRefreshes.RefreshCompletely();
                })
                .CompositeDisposalWith(_compositeDisposable);

            EmptyCellsAvgOutflowsLastYear = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToAvgOutflow(12);
                    budgetRefreshes.RefreshCompletely();
                })
                .CompositeDisposalWith(_compositeDisposable);

            EmptyCellsBalanceToZero = new RxRelayCommand(async () =>
                {
                    await budgetMonth.EmptyBudgetEntriesToBalanceZero();
                    budgetRefreshes.RefreshCompletely();
                })
                .CompositeDisposalWith(_compositeDisposable);

            AllCellsZero = new RxRelayCommand(async () =>
                {
                    await budgetMonth.AllBudgetEntriesToZero();
                    budgetRefreshes.RefreshCompletely();
                })
                .CompositeDisposalWith(_compositeDisposable);
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
        public ICommand EmptyCellsBudgetLastMonth { get; }
        public ICommand EmptyCellsOutflowsLastMonth { get; }
        public ICommand EmptyCellsAvgOutflowsLastThreeMonths { get; }
        public ICommand EmptyCellsAvgOutflowsLastYear { get; }
        public ICommand EmptyCellsBalanceToZero { get; }
        public ICommand AllCellsZero { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
