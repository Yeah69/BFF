using System;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ITransaction : ITransInc {}

    /// <summary>
    /// The Transaction documents payment to or from externals
    /// </summary>
    public class Transaction : TransInc, ITransaction
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transaction(DateTime date, Account account = null, Payee payee = null, Category category = null, string memo = null, 
            long sum = 0L, bool? cleared = null)
            : base(date, account, payee, category, memo, sum, cleared)
        {
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="categoryId">Id of Category</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transaction(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
            : base(id, accountId, date, payeeId, categoryId, memo, sum, cleared)
        {
        }
    }
}
