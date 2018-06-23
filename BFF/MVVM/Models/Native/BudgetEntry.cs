﻿using System;
using System.Threading.Tasks;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IBudgetEntry : IDataModel
    {
        ICategory Category { get; set; }
        DateTime Month { get; }
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }
    }
    
    public class BudgetEntry : DataModel<IBudgetEntry>, IBudgetEntry
    {
        public BudgetEntry(
            IWriteOnlyRepository<IBudgetEntry> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id, 
            DateTime month,
            ICategory category = null,
            long budget = 0L,
            long outflow = 0L,
            long balance = 0L)
            : base(repository, rxSchedulerProvider, id)
        {
            Month = month;
            _category = category;
            _budget = budget;
            _outflow = outflow;
            _balance = balance;
        }

        public DateTime Month { get; }
        
        private ICategory _category;

        public ICategory Category
        {
            get => _category;
            set
            {
                if(_category == value) return;

                _category = value;
                UpdateAndNotify();
            }
        }

        private long _budget;

        public long Budget
        {
            get => _budget;
            set
            {
                if(_budget == value) return;

                if (_budget == 0 && Id == -1)
                {
                    _budget = value;
                    Task.Run(InsertAsync)
                        .ContinueWith(_ => OnPropertyChanged());
                }
                else if (_budget != 0 && value == 0 && Id > -1)
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

        private long _outflow;

        public long Outflow
        {
            get => _outflow;
            set
            {
                if (_outflow == value) return;

                _outflow = value;
                UpdateAndNotify();
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
                UpdateAndNotify();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Month)}: {Month}, {nameof(Category)}: {Category}, {nameof(Budget)}: {Budget}, {nameof(Outflow)}: {Outflow}, {nameof(Balance)}: {Balance}";
        }
    }
}
