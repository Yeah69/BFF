using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using MrMeeseeks.Extensions;

namespace BFF.Model.Models
{
    public interface IBudgetEntry : IDataModel
    {
        ICategory Category { get; }
        DateTime Month { get; }
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }
        Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();
        Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync();
    }

    public abstract class BudgetEntry : DataModel, IBudgetEntry
    {
        public BudgetEntry(
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime month,
            ICategory category,
            long budget,
            long outflow,
            long balance)
            : base(rxSchedulerProvider)
        {
            Month = month;
            Category = category;
            _budget = budget;
            Outflow = outflow;
            Balance = balance;
        }

        public DateTime Month { get; }

        public ICategory Category { get; }

        private long _budget;

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
            }
        }

        public long Outflow { get; }

        public long Balance { get; }

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync();

        public override string ToString()
        {
            return $"{nameof(Month)}: {Month}, {nameof(Category)}: {Category}, {nameof(Budget)}: {Budget}, {nameof(Outflow)}: {Outflow}, {nameof(Balance)}: {Balance}";
        }
    }
}
