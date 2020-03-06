using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{

    public interface IBudgetMonth : INotifyPropertyChanged
    {
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
        Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();
        Task<IEnumerable<ITransBase>> GetAssociatedTransForIncomeCategoriesAsync();
    }

    public abstract class BudgetMonth : ObservableObject, IBudgetMonth
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
            var _budgetEntries = new ObservableCollection<IBudgetEntry>(budgetEntries.OrderBy(be => be.Category, new CategoryComparer()));

            BudgetedThisMonth = - _budgetEntries.Sum(be => be.Budget);
            Outflows = _budgetEntries.Sum(be => be.Outflow);
            Balance = _budgetEntries.Sum(be => be.Balance);

            AvailableToBudget = NotBudgetedInPreviousMonth +
                OverspentInPreviousMonth +
                IncomeForThisMonth +
                BudgetedThisMonth +
                DanglingTransferForThisMonth +
                UnassignedTransactionSumForThisMonth;
        }

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

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();
        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransForIncomeCategoriesAsync();
    }
}
