﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;

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
            _outflow = outflow;
            _balance = balance;
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

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransAsync();

        public abstract Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync();

        public override string ToString()
        {
            return $"{nameof(Month)}: {Month}, {nameof(Category)}: {Category}, {nameof(Budget)}: {Budget}, {nameof(Outflow)}: {Outflow}, {nameof(Balance)}: {Balance}";
        }
    }
}
