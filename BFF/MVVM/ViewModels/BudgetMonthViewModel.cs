using System.Collections.ObjectModel;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.ViewModels
{
    public interface IBudgetMonthViewModel
    {
        ObservableCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        long NotBudgetedInPreviousMonth { get; }
        long OverspentInPreviousMonth { get; }
        long IncomeForThisMonth { get; }
        long BudgetedThisMonth { get; }
        long AvailableToBudget { get; }
        long Outflows { get; }
        long Balance { get; }
    }

    public class BudgetMonthViewModel : IBudgetMonthViewModel
    {
        public BudgetMonthViewModel(
            ObservableCollection<IBudgetEntryViewModel> budgetEntries,
            long notBudgetedInPreviousMonth, 
            long overspentInPreviousMonth, 
            long incomeForThisMonth,
            long budgetedThisMonth, 
            long outflows, 
            long balance)
        {
            BudgetEntries = budgetEntries;
            NotBudgetedInPreviousMonth = notBudgetedInPreviousMonth;
            OverspentInPreviousMonth = overspentInPreviousMonth;
            IncomeForThisMonth = incomeForThisMonth;
            BudgetedThisMonth = budgetedThisMonth;
            Outflows = outflows;
            Balance = balance;
        }

        public ObservableCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        public long NotBudgetedInPreviousMonth { get; }
        public long OverspentInPreviousMonth { get; }
        public long IncomeForThisMonth { get; }
        public long BudgetedThisMonth { get; }

        public long AvailableToBudget => 
            NotBudgetedInPreviousMonth + 
            OverspentInPreviousMonth + 
            IncomeForThisMonth +
            BudgetedThisMonth;

        public long Outflows { get; }
        public long Balance { get; }
    }
}
