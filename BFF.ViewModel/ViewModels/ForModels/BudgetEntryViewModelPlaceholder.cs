using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.ForModels.Utility;

namespace BFF.ViewModel.ViewModels.ForModels
{
    internal class BudgetEntryViewModelPlaceholder : NotifyingErrorViewModelBase, IBudgetEntryViewModel
    {
        internal static BudgetEntryViewModelPlaceholder Instance => new BudgetEntryViewModelPlaceholder();

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

        public IRxRelayCommand DeleteCommand => null;
        public bool IsInserted => false;
        public ICategoryViewModel Category => null;
        public DateTime Month => DateTime.MinValue;
        public long Budget { get; set; } = 0L;
        public long Outflow => 0L;
        public long Balance => 0L;
        public long AggregatedBudget => 0L;
        public long AggregatedOutflow => 0L;
        public long AggregatedBalance => 0L;
        public IReadOnlyList<IBudgetEntryViewModel> Children { get; set; } = new List<IBudgetEntryViewModel>();
        public ILazyTransLikeViewModels AssociatedTransElementsViewModel => null;
        public ILazyTransLikeViewModels AssociatedAggregatedTransElementsViewModel => null;
        public ICommand BudgetLastMonth => null;
        public ICommand OutflowsLastMonth => null;
        public ICommand AvgOutflowsLastThreeMonths => null;
        public ICommand AvgOutflowsLastYear => null;
        public ICommand BalanceToZero => null;
        public ICommand Zero => null;
        public Task SetBudgetToAverageBudgetOfLastMonths(int monthCount) => Task.CompletedTask;

        public Task SetBudgetToAverageOutflowOfLastMonths(int monthCount) => Task.CompletedTask;
    }
}
