using System;
using System.Threading;
using System.Windows.Input;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.ViewModels
{
    public class BudgetMonthViewModelPlaceholder : IBudgetMonthViewModel
    {
        public BudgetMonthViewModelPlaceholder(
            DateTime month)
        {
            Month = month;
        }
        public DateTime Month { get; }

        public string MonthName =>
            Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(this.Month.Month);
        public long NotBudgetedInPreviousMonth => 0L;
        public long OverspentInPreviousMonth => 0L;
        public long IncomeForThisMonth => 0L;
        public long DanglingTransferForThisMonth => 0L;
        public long UnassignedTransactionSumForThisMonth => 0L;
        public long BudgetedThisMonth => 0L;
        public long BudgetedThisMonthPositive => 0L;
        public long AvailableToBudget => 0L;
        public long Outflows => 0L;
        public long Balance => 0L;
        public ILazyTransLikeViewModels? AssociatedTransElementsViewModel => null;
        public ILazyTransLikeViewModels? AssociatedIncomeTransElementsViewModel => null;
        public ICommand EmptyCellsBudgetLastMonth => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand EmptyCellsOutflowsLastMonth => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand EmptyCellsAvgOutflowsLastThreeMonths => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand EmptyCellsAvgOutflowsLastYear => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand EmptyCellsBalanceToZero => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand AllCellsZero => RxCommand.CanAlwaysExecuteNeverEmits();
    }
}
