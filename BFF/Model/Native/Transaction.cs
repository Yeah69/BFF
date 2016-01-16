using System;
using System.Collections.Generic;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    public class Transaction : TitNoTransfer
    {
        /// <summary>
        /// SubElements if this is a Parent
        /// </summary>
        [Write(false)]
        public override IEnumerable<SubTitBase> SubElements => Type == "SingleTrans" ? new List<SubTransaction>(0) : Database?.GetSubTransInc<SubTransaction>(Id);

        /// <summary>
        /// Indicates if it is a Single or Parent
        /// </summary>
        public override string Type { get; set; } = "SingleTrans";
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        /// <param name="type"></param>
        public Transaction(DateTime date, Account account = null, Payee payee = null,
            Category category = null, string memo = null, bool? cleared = null, string type = "SingleTrans")
            : base(account, payee, category, memo, cleared)
        {
            ConstrDbLock = true;

            Date = date;
            Type = type;

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
        public Transaction(long id, long accountId, long payeeId, long categoryId, DateTime date, string memo,
            long? sum, bool cleared, string type)
            : base(id, accountId, payeeId, categoryId, memo, sum, cleared)
        {
            ConstrDbLock = true;

            Date = date;
            Type = type;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static implicit operator Transaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = Category.GetOrCreate(ynabTransaction.Category);
            Transaction ret = new Transaction(ynabTransaction.Date)
            {
                Account = Account.GetOrCreate(ynabTransaction.Account),
                Payee = Payee.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value),
                Category = tempCategory,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }

        protected override void DbUpdate()
        {
            Database?.Update(this);
        }
    }
}
