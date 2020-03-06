using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;

namespace BFF.Model.Models
{
    public interface IBudgetCategory
    {
        ICategory Category { get; }
        
        Task<IEnumerable<IBudgetEntry>> GetBudgetEntriesFor(int year);
    }
    
    public abstract class BudgetCategory : ObservableObject, IBudgetCategory
    {
        protected BudgetCategory(ICategory category)
        {
            Category = category;
        }
        
        public ICategory Category { get; }

        public abstract Task<IEnumerable<IBudgetEntry>> GetBudgetEntriesFor(int year);
    }
}