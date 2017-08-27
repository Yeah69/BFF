using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IBudgetEntry : IDataModel, IHaveCategory
    {
        DateTime Month { get; set; }
        long Budget { get; set; }
    }

    public class BudgetEntry : DataModel<IBudgetEntry>, IBudgetEntry
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="month">The month of the budget entry</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="budget">The amount of money, which was budgeted in the set month</param>
        public BudgetEntry(IRepository<IBudgetEntry> repository, long id, DateTime month, ICategory category = null, long budget = 0L)
            : base(repository, id)
        {
            _month = month;
            _category = category;
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

        private ICategory _category;

        public ICategory Category
        {
            get => _category;
            set
            {
                if(_category == value) return;

                _category = value;
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
