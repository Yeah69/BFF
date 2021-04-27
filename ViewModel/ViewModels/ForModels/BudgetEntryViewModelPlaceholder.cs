using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public class BudgetEntryViewModelPlaceholder : NotifyingErrorViewModelBase, IBudgetEntryViewModel
    {
        internal static BudgetEntryViewModelPlaceholder Instance => new();

        private BudgetEntryViewModelPlaceholder() { }

        public Task InsertAsync()
        {
            return Task.CompletedTask;
        }

        public bool IsInsertable()
        {
            return false;
        }

        public Task DeleteAsync()
        {
            return Task.CompletedTask;
        }

        public ICommand? DeleteCommand => null;
        public bool IsInserted => false;
        public ICategoryViewModel? Category => null;
        public DateTime Month => DateTime.MinValue;
        public long Budget { get; set; } = 0L;
        public long Outflow => 0L;
        public long Balance => 0L;
        public long AggregatedBudget => 0L;
        public long AggregatedOutflow => 0L;
        public long AggregatedBalance => 0L;
        public IReadOnlyList<IBudgetEntryViewModel> Children { get; set; } = new List<IBudgetEntryViewModel>();
        public ILazyTransLikeViewModels? AssociatedTransElementsViewModel => null;
        public ILazyTransLikeViewModels? AssociatedAggregatedTransElementsViewModel => null;
        public ICommand BudgetLastMonth => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand OutflowsLastMonth => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand AvgOutflowsLastThreeMonths => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand AvgOutflowsLastYear => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand BalanceToZero => RxCommand.CanAlwaysExecuteNeverEmits();
        public ICommand Zero => RxCommand.CanAlwaysExecuteNeverEmits();
        public Task SetBudgetToAverageBudgetOfLastMonths(int monthCount) => Task.CompletedTask;

        public Task SetBudgetToAverageOutflowOfLastMonths(int monthCount) => Task.CompletedTask;
    }
}
