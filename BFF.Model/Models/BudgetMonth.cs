using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using BFF.Core.Helper;
using BFF.Model.Repositories.ModelRepositories;

namespace BFF.Model.Models
{

    public interface IBudgetMonth : INotifyPropertyChanged
    {
        ObservableCollection<IBudgetEntry> BudgetEntries { get; }
        DateTime Month { get; }
        long NotBudgetedInPreviousMonth { get; }
        long OverspentInPreviousMonth { get; }
        long IncomeForThisMonth { get; }
        long DanglingTransferForThisMonth { get; }
        long UnassignedTransactionSumForThisMonth { get; }
        long BudgetedThisMonth { get; }
        long AvailableToBudget { get; }
        long Outflows { get; }
        long Balance { get; }
    }

    internal class BudgetMonth : ObservableObject, IBudgetMonth
    {
        public BudgetMonth(
            DateTime month,
            IEnumerable<IBudgetEntry> budgetEntries, 
            long overspentInPreviousMonth,
            long notBudgetedInPreviousMonth,
            long incomeForThisMonth,
            long danglingTransferForThisMonth, 
            long unassignedTransactionSumForThisMonth)
        {
            Month = month;
            OverspentInPreviousMonth = overspentInPreviousMonth;
            NotBudgetedInPreviousMonth = notBudgetedInPreviousMonth;
            IncomeForThisMonth = incomeForThisMonth;
            DanglingTransferForThisMonth = danglingTransferForThisMonth;
            UnassignedTransactionSumForThisMonth = unassignedTransactionSumForThisMonth;
            BudgetEntries = new ObservableCollection<IBudgetEntry>(budgetEntries.OrderBy(be => be.Category, new CategoryComparer()));

            BudgetedThisMonth = - BudgetEntries.Sum(be => be.Budget);
            Outflows = BudgetEntries.Sum(be => be.Outflow);
            Balance = BudgetEntries.Sum(be => be.Balance);

            AvailableToBudget = NotBudgetedInPreviousMonth +
                OverspentInPreviousMonth +
                IncomeForThisMonth +
                BudgetedThisMonth +
                DanglingTransferForThisMonth +
                UnassignedTransactionSumForThisMonth;
        }

        public ObservableCollection<IBudgetEntry> BudgetEntries { get; }
        public DateTime Month { get; }
        public long NotBudgetedInPreviousMonth { get; }
        public long OverspentInPreviousMonth { get; }
        public long IncomeForThisMonth { get; }
        public long DanglingTransferForThisMonth { get; }
        public long UnassignedTransactionSumForThisMonth { get; }
        public long BudgetedThisMonth { get; }
        public long AvailableToBudget { get; }
        public long Outflows { get; }
        public long Balance { get; }

        public override string ToString()
        {
            return $"{nameof(BudgetEntries)}: {BudgetEntries}, {nameof(Month)}: {Month}, {nameof(NotBudgetedInPreviousMonth)}: {NotBudgetedInPreviousMonth}, {nameof(OverspentInPreviousMonth)}: {OverspentInPreviousMonth}, {nameof(IncomeForThisMonth)}: {IncomeForThisMonth}, {nameof(DanglingTransferForThisMonth)}: {DanglingTransferForThisMonth}, {nameof(BudgetedThisMonth)}: {BudgetedThisMonth}, {nameof(AvailableToBudget)}: {AvailableToBudget}, {nameof(Outflows)}: {Outflows}, {nameof(Balance)}: {Balance}";
        }
    }
}
