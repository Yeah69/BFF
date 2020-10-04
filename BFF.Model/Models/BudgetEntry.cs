using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using MrMeeseeks.Extensions;

namespace BFF.Model.Models
{
    public interface IBudgetEntry : IDataModel
    {
        ICategory Category { get; }
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }
        long AggregatedBudget { get; }
        long AggregatedOutflow { get; }
        long AggregatedBalance { get; }
        Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();
        Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync();

        Task SetBudgetToAverageBudgetOfLastMonths(int monthCount);
        Task SetBudgetToAverageOutflowOfLastMonths(int monthCount);
    }

    public abstract class BudgetEntry : DataModel, IBudgetEntry
    {
        public BudgetEntry(
            // parameters
            DateTime month,
            ICategory category,
            long budget,
            long outflow,
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance,
            
            // dependencies
            IClearBudgetCache clearBudgetCache,
            IUpdateBudgetCategory updateBudgetCategory)
            : base()
        {
            Month = month;
            Category = category;
            _budget = budget;
            _clearBudgetCache = clearBudgetCache;
            _updateBudgetCategory = updateBudgetCategory;
            Outflow = outflow;
            Balance = balance;
            AggregatedBudget = aggregatedBudget;
            AggregatedOutflow = aggregatedOutflow;
            AggregatedBalance = aggregatedBalance;
        }

        protected DateTime Month { get; }

        public ICategory Category { get; }

        private long _budget;
        private readonly IClearBudgetCache _clearBudgetCache;
        private readonly IUpdateBudgetCategory _updateBudgetCategory;

        public long Budget
        {
            get => _budget;
            set
            {
                if(_budget == value) return;

                if (_budget == 0 && IsInserted.Not())
                {
                    _budget = value;
                    Task.Run(InsertAsync)
                        .ContinueWith(_ => OnPropertyChanged());
                }
                else if (_budget != 0 && value == 0 && IsInserted)
                {
                    _budget = value;
                    Task.Run(DeleteAsync)
                        .ContinueWith(_ => OnPropertyChanged());
                }
                else
                {
                    _budget = value;
                    UpdateAndNotify();
                }
                _clearBudgetCache.Clear();
                _updateBudgetCategory.UpdateCategory(Category);
            }
        }

        public long Outflow { get; }

        public long Balance { get; }
        
        public long AggregatedBudget { get; }
        
        public long AggregatedOutflow { get; }
        
        public long AggregatedBalance { get; }

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync();
        public async Task SetBudgetToAverageBudgetOfLastMonths(int monthCount)
        {
            Budget = await AverageBudgetOfLastMonths(monthCount).ConfigureAwait(false);
        }

        public async Task SetBudgetToAverageOutflowOfLastMonths(int monthCount)
        {
            Budget = - await AverageOutflowOfLastMonths(monthCount).ConfigureAwait(false);
        }

        protected abstract Task<long> AverageBudgetOfLastMonths(int monthCount);
        protected abstract Task<long> AverageOutflowOfLastMonths(int monthCount);
    }
}
