﻿using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IParentTransaction : IParentTransInc {}

    /// <summary>
    /// A Transaction, which is split into several SubTransactions
    /// </summary>
    public class ParentTransaction : ParentTransInc<ParentTransaction>, IParentTransaction
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(IRepository<ParentTransaction> repository, 
                                 DateTime date, 
                                 IAccount account = null, 
                                 IPayee payee = null, 
                                 string memo = null, 
                                 bool? cleared = null)
            : base(repository, date, account, payee, memo, cleared) {}

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(IRepository<ParentTransaction> repository, 
                                 long id,
                                 long accountId, 
                                 DateTime date, 
                                 long payeeId, 
                                 string memo,
                                 bool cleared)
            : base(repository, id, accountId, date, payeeId, memo, cleared) { }
    }
}
