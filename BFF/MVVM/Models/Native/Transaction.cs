﻿using System;
using BFF.Core;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ITransaction : ITransactionBase, IHaveCategory
    {
        long Sum { get; set; }
    }
    
    public class Transaction : TransactionBase<ITransaction>, ITransaction
    {
        private ICategoryBase _category;
        private long _sum;
        
        public Transaction(
            IRepository<ITransaction> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime date,
            long id = -1L,
            IFlag flag = null,
            string checkNumber = "",
            IAccount account = null, 
            IPayee payee = null, 
            ICategoryBase category = null,
            string memo = "", 
            long sum = 0L, 
            bool? cleared = false)
            : base(repository, rxSchedulerProvider, id, flag, checkNumber, date, account, payee, memo, cleared)
        {
            _category = category;
            _sum = sum;
        }
        
        public ICategoryBase Category
        {
            get => _category;
            set
            {
                if (value is null)
                {
                    OnPropertyChanged();
                    return;
                }
                if(_category == value) return;
                _category = value;
                UpdateAndNotify();
            }
        }
        
        public long Sum
        {
            get => _sum;
            set
            {
                if(_sum == value) return;
                _sum = value;
                UpdateAndNotify();
            }
        }
    }
}
