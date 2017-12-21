﻿using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ITransaction : ITransInc {}

    /// <summary>
    /// The Transaction documents payment to or from externals
    /// </summary>
    public class Transaction : TransInc<ITransaction>, ITransaction
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payed or who payed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payed or received</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transaction(
            IRepository<ITransaction> repository,
            long id,
            DateTime date, 
            IAccount account = null, 
            IPayee payee = null, 
            ICategoryBase category = null,
            string memo = null, 
            long sum = 0L, 
            bool? cleared = null)
            : base(repository, id, date, account, payee, category, memo, sum, cleared)
        {
        }
    }
}
