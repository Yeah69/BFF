﻿using System;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IParentTransaction : IParentTransInc {}

    /// <summary>
    /// A Transaction, which is split into several SubTransactions
    /// </summary>
    public class ParentTransaction : ParentTransInc, IParentTransaction
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(DateTime date, Account account = null, Payee payee = null, string memo = null, bool? cleared = null)
            : base(date, account, payee, memo, cleared) {}

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
            : base(id, accountId, date, payeeId, memo, cleared) {}
    }
}
