using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// A Transaction, which is split into several SubTransactions
    /// </summary>
    public class ParentTransaction : Transaction, IParentTransInc<SubTransaction>
    {
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
            : base(date, account, payee, category, memo, 0L, cleared)
        {
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="categoryId">Id of Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo,
            long sum, bool cleared)
            : base(id, accountId, date, payeeId, categoryId, memo, sum, cleared)
        {
        }

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static explicit operator ParentTransaction(Conversion.YNAB.Transaction ynabTransaction)
        {
            ParentTransaction ret = new ParentTransaction(ynabTransaction.Date)
            {
                AccountId = Account.GetOrCreate(ynabTransaction.Account)?.Id ?? -1,
                PayeeId = Payee.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value)?.Id ?? -1,
                CategoryId = -1,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = 0L,
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }
    }
}
