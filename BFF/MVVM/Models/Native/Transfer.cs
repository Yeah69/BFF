﻿using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ITransfer : ITitBase {
        /// <summary>
        /// Id of FromAccount
        /// </summary>
        long FromAccountId { get; set; }

        /// <summary>
        /// Id of ToAccount
        /// </summary>
        long ToAccountId { get; set; }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        long Sum { get; set; }
    }

    /// <summary>
    /// A Transfer is basically a Transaction from one owned Account to another owned Account
    /// </summary>
    public class Transfer : TitBase<Transfer>, ITransfer
    {
        private long _fromAccountId;
        private long _toAccountId;
        private long _sum;

        /// <summary>
        /// Id of FromAccount
        /// </summary>
        public long FromAccountId
        {
            get => _fromAccountId;
            set
            {
                if(_fromAccountId == value) return;
                _fromAccountId = value; 
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of ToAccount
        /// </summary>
        public long ToAccountId
        {
            get => _toAccountId;
            set
            {
                if(_toAccountId == value) return;
                _toAccountId = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public long Sum
        {
            get => _sum;
            set
            {
                if(_sum == value) return;
                _sum = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="fromAccount">The Sum is transfered from this Account</param>
        /// <param name="toAccount">The Sum is transfered to this Account</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The transfered Sum</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transfer(IRepository<Transfer> repository, 
                        DateTime date, 
                        IAccount fromAccount = null, 
                        IAccount toAccount = null, 
                        string memo = null,
                        long sum = 0L,
                        bool? cleared = null)
            : base(repository, date, memo: memo, cleared: cleared)
        {
            _fromAccountId = fromAccount?.Id ?? -1;
            _toAccountId = toAccount?.Id ?? -1;
            _sum = sum;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="fromAccountId">Id of FromAccount</param>
        /// <param name="toAccountId">Id of ToAccount</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The transfered Sum</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transfer(IRepository<Transfer> repository,
                        long id,
                        long fromAccountId, 
                        long toAccountId, 
                        DateTime date, 
                        string memo,
                        long sum,
                        bool cleared)
            : base(repository, date, id, memo, cleared)
        {
            _fromAccountId = fromAccountId;
            _toAccountId = toAccountId;
            _sum = sum;
        }
    }
}
