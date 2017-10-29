using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BFF.MVVM.Models.Native
{

    public interface IBudgetMonth
    {
        ObservableCollection<IBudgetEntry> BudgetEntries { get; }
        long NotBudgetedInPreviousMonth { get; }
        long OverspentInPreviousMonth { get; }
        long IncomeForThisMonth { get; }
        long BudgetedThisMonth { get; }
        long AvailableToBudget { get; }
        long Outflows { get; }
        long Balance { get; }
    }

    public class BudgetMonth : IBudgetMonth
    {
        public BudgetMonth(IEnumerable<IBudgetEntry> budgetEntries)
        {
            BudgetEntries = new ObservableCollection<IBudgetEntry>(budgetEntries);
        }

        public ObservableCollection<IBudgetEntry> BudgetEntries { get; }
        public long NotBudgetedInPreviousMonth { get; }
        public long OverspentInPreviousMonth { get; }
        public long IncomeForThisMonth { get; }
        public long BudgetedThisMonth { get; }
        public long AvailableToBudget { get; }
        public long Outflows { get; }
        public long Balance { get; }
    }
}
