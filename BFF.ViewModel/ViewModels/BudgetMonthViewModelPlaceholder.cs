﻿using System;
using System.Linq;
using System.Reactive.Linq;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels
{
    internal class BudgetMonthViewModelPlaceholder : IBudgetMonthViewModel
    {
        internal BudgetMonthViewModelPlaceholder(
            DateTime month,
            int budgetEntriesCount)
        {
            Month = month;
            BudgetEntries = Enumerable
                .Range(0, budgetEntriesCount)
                .Select(_ => BudgetEntryViewModelPlaceholder.Instance)
                .ToReadOnlyReactiveCollection(Observable.Never<CollectionChanged<IBudgetEntryViewModel>>());
        }

        public ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        public DateTime Month { get; }
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
        public ILazyTransLikeViewModels AssociatedTransElementsViewModel => null;
        public ILazyTransLikeViewModels AssociatedIncomeTransElementsViewModel => null;
        public IRxRelayCommand EmptyCellsBudgetLastMonth => null;
        public IRxRelayCommand EmptyCellsOutflowsLastMonth => null;
        public IRxRelayCommand EmptyCellsAvgOutflowsLastThreeMonths => null;
        public IRxRelayCommand EmptyCellsAvgOutflowsLastYear => null;
        public IRxRelayCommand EmptyCellsBalanceToZero => null;
        public IRxRelayCommand AllCellsZero => null;
    }
}
