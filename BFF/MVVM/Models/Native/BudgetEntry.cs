using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IBudgetEntry : IDataModel
    {
        DateTime Month { get; }
        long CategoryId { get; }
        long Budget { get; set; }
    }

    public class BudgetEntry : DataModel<BudgetEntry>, IBudgetEntry, IHaveCategory
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="month">The month of the budget entry</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="budget">The amount of money, which was budgeted in the set month</param>
        public BudgetEntry(IRepository<BudgetEntry> repository, DateTime month, ICategory category = null, long budget = 0L)
            : base(repository)
        {
            _month = month;
            _categoryId = category?.Id ?? -1;
            _budget = budget;
        }

        private DateTime _month;

        public DateTime Month
        {
            get => _month;
            set
            {
                if(_month == value) return;

                _month = value;
                Update();
                OnPropertyChanged();
            }
        }

        private long _categoryId;

        public long CategoryId
        {
            get => _categoryId;
            set
            {
                if(_categoryId == value) return;

                _categoryId = value;
                Update();
                OnPropertyChanged();
            }
        }

        private long _budget;

        public long Budget
        {
            get => _budget;
            set
            {
                if(_budget == value) return;

                _budget = value;
                Update();
                OnPropertyChanged();
            }
        }
    }
}
