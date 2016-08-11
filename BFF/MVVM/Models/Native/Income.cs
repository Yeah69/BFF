using System;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// The Income documents earned money
    /// </summary>
    public class Income : TitNoTransfer, ITransInc
    {
        private TitNoTransfer _parent;

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
        public Income(DateTime date, Account account = null, Payee payee = null,
            Category category = null, string memo = null, long sum = 0L, bool? cleared = null)
            : base(date, account, payee, category, memo, sum, cleared)
        {
            ConstrDbLock = true;

            Date = date;

            ConstrDbLock = false;
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
        public Income(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo,
            long sum, bool cleared)
            : base(id, accountId, date, payeeId, categoryId, memo, sum, cleared)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Creates a Income-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static implicit operator Income(Conversion.YNAB.Transaction ynabTransaction)
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

        protected override void InsertToDb()
        {
            Database?.Insert(this);
        }

        protected override void UpdateToDb()
        {
            Database?.Update(this);
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        }

        protected override void DeleteFromDb()
        {
            Database?.Delete(this);
        }
    }
}
