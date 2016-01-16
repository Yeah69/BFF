using System;
using System.Collections.Generic;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    public class Income : TitNoTransfer
    {
        /// <summary>
        /// SubElements if this is a Parent
        /// </summary>
        [Write(false)]
        public override IEnumerable<SubTitBase> SubElements => Type == "SingleIncome" ? new List<SubIncome>(0) : Database?.GetSubTransInc<SubIncome>(Id);

        /// <summary>
        /// Indicates if it is a Single or Parent
        /// </summary>
        public override string Type { get; set; } = "SingleIncome";

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
        public Income(DateTime date, Account account = null, Payee payee = null,
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
        public Income(long id, long accountId, long payeeId, long categoryId, DateTime date, string memo,
            long? sum, bool cleared, string type)
            : base(id, accountId, payeeId, categoryId, memo, sum, cleared)
        {
            ConstrDbLock = true;

            Date = date;
            Type = type;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Creates a Income-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static implicit operator Income(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = Category.GetOrCreate(ynabTransaction.Category);
            Income ret = new Income (ynabTransaction.Date)
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
