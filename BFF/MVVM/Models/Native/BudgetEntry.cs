using System;
using System.Diagnostics;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IBudgetEntry : IDataModel, IHaveCategory
    {
        DateTime Month { get; set; }
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }
    }
    
    public class BudgetEntry : DataModel<IBudgetEntry>, IBudgetEntry
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="month">The month of the budget entry</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="budget">The amount of money, which was budgeted in the set month</param>
        public BudgetEntry(
            IWriteOnlyRepository<IBudgetEntry> repository, 
            long id, DateTime month,
            ICategory category = null,
            long budget = 0L,
            long outflow = 0L,
            long balance = 0L)
            : base(repository, id)
        {
            _month = month;
            _category = category;
            _budget = budget;
            _outflow = outflow;
            _balance = balance;
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

        private long _outflow;

        public long Outflow
        {
            get => _outflow;
            set
            {
                if (_outflow == value) return;

                _outflow = value;
                Update();
                OnPropertyChanged();
            }
        }

        private long _balance;

        public long Balance
        {
            get => _balance;
            set
            {
                if (_balance == value) return;

                _balance = value;
                Update();
                OnPropertyChanged();
            }
        }
        public override string ToString()
        {
            return $"{nameof(Month)}: {Month}, {nameof(Category)}: {Category}, {nameof(Budget)}: {Budget}, {nameof(Outflow)}: {Outflow}, {nameof(Balance)}: {Balance}";
        }
    }
}
