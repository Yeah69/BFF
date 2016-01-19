using System;
using System.Collections.Generic;
using System.Linq;
using BFF.Helper.Import;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    public class ParentTransaction : Transaction
    {
        /// <summary>
        /// SubElements if this is a Parent
        /// </summary>
        [Write(false)]
        public  IEnumerable<SubTransaction> SubElements => Database?.GetSubTransInc<SubTransaction>(Id);

        public override long? Sum
        {
            get { return SubElements.Sum(subElement => subElement.Sum); } //todo: Write an SQL query for that
            set { }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(DateTime date, Account account = null, Payee payee = null,
            Category category = null, string memo = null, bool? cleared = null)
            : base(date, account, payee, category, memo, cleared)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="categoryId">Id of Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(long id, long accountId, long payeeId, long categoryId, DateTime date, string memo,
            long? sum, bool cleared)
            : base(id, accountId, payeeId, categoryId, date, memo, sum, cleared)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static explicit operator ParentTransaction(YNAB.Transaction ynabTransaction)
        {
            ParentTransaction ret = new ParentTransaction(ynabTransaction.Date)
            {
                Account = Account.GetOrCreate(ynabTransaction.Account),
                Payee = Payee.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value),
                Category = null,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = null,
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }
    }
}
