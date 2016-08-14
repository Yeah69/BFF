using System;

namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Base of all Tit-classes except Transfer (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TitNoTransfer : TitBase
    {
        private long _accountId;
        private long _payeeId;
        private long _categoryId;

        /// <summary>
        /// Id of Account
        /// </summary>
        public long AccountId
        {
            get { return _accountId; }
            set
            {
                _accountId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Payee
        /// </summary>
        public long PayeeId
        {
            get { return _payeeId; }
            set
            {
                _payeeId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Category
        /// </summary>
        public long CategoryId
        {
            get { return _categoryId; }
            set
            {
                _categoryId = value;
                OnPropertyChanged();
            }
        }

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
        protected TitNoTransfer(DateTime date, Account account = null, Payee payee = null,
            Category category = null, string memo = null, long sum = 0L, bool? cleared = null)
            : base(date, memo: memo, sum: sum, cleared: cleared)
        {
            _accountId = account?.Id ?? -1;
            _payeeId = payee?.Id ?? -1;
            _categoryId = category?.Id ?? -1;
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
        protected TitNoTransfer(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo,
            long sum, bool cleared)
            : base(date, id, memo, sum, cleared)
        {
            AccountId = accountId;
            PayeeId = payeeId;
            CategoryId = categoryId;
        }
    }
}
