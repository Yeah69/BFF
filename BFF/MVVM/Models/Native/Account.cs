﻿using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IAccount : ICommonProperty
    {
        long StartingBalance { get; set; }

        DateTime StartingDate { get; set; }
    }
    
    public class Account : CommonProperty<IAccount>, IAccount
    {
        private long _startingBalance;
        private DateTime _startingDate;
        
        public virtual long StartingBalance
        {
            get => _startingBalance;
            set
            {
                if(_startingBalance == value) return;
                _startingBalance = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }

        public DateTime StartingDate
        {
            get => _startingDate;
            set
            {
                if (_startingDate == value) return;
                _startingDate = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }
        
        public Account(IRepository<IAccount> repository,
            DateTime startingDate, 
            long id = -1L, 
            string name = "", 
            long startingBalance = 0L) 
            : base(repository, name: name)
        {
            Id = id;
            _startingBalance = startingBalance;
            _startingDate = startingDate;
        }
    }
}
