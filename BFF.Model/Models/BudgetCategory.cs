using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.Model.Models
{
    public interface IBudgetCategory
    {
        IObservable<Unit> ObserveUpdateSignal { get; }

        Task<IEnumerable<IBudgetEntry>> GetBudgetEntriesFor(int year);
    }
    
    public abstract class BudgetCategory : IBudgetCategory
    {
        private readonly IObserveUpdateBudgetCategory _observeUpdateBudgetCategory;

        protected BudgetCategory(
            // parameters
            ICategory category,
            
            // dependencies
            IObserveUpdateBudgetCategory observeUpdateBudgetCategory)
        {
            _observeUpdateBudgetCategory = observeUpdateBudgetCategory;
            Category = category;
        }
        
        protected ICategory Category { get; }

        public IObservable<Unit> ObserveUpdateSignal =>
            _observeUpdateBudgetCategory
                .Observe
                // Check whether signaled category is ancestor of my category or my category
                .Where(c => 
                    MoreLinq
                        .MoreEnumerable
                        .Generate(c, cat => cat?.Parent)
                        .TakeWhileNotNull()
                        .Any(cat => cat == Category))
                .SelectUnit();
        
        public abstract Task<IEnumerable<IBudgetEntry>> GetBudgetEntriesFor(int year);
    }
}