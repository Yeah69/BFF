using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        Task EmptyBudgetEntriesToAvgBudget(int monthCount);
        Task EmptyBudgetEntriesToAvgOutflow(int monthCount);
        Task EmptyBudgetEntriesToBalanceZero();
        Task AllBudgetEntriesToZero();
    }

    public abstract class BudgetMonth : ObservableObject, IBudgetMonth
    {
        public BudgetMonth(
            DateTime month,
            (long Budget, long Outflow, long Balance) budgetData, 
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

            BudgetedThisMonth = - budgetData.Budget;
            Outflows = budgetData.Outflow;
            Balance = budgetData.Balance;

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
        public abstract Task EmptyBudgetEntriesToAvgBudget(int monthCount);
        public abstract Task EmptyBudgetEntriesToAvgOutflow(int monthCount);
        public abstract Task EmptyBudgetEntriesToBalanceZero();
        public abstract Task AllBudgetEntriesToZero();
    }
}
