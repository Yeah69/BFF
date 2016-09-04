using System;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransIncBase : ITitBase
    {
        /// <summary>
        /// Id of Account
        /// </summary>
        long AccountId { get; set; }

        /// <summary>
        /// Id of Payee
        /// </summary>
        long PayeeId { get; set; }
    }

    /// <summary>
    /// Base of all Tit-classes except Transfer (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TransIncBase : TitBase, ITransIncBase
    {
        private long _accountId;
        private long _payeeId;

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
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TransIncBase(DateTime date, IAccount account = null, IPayee payee = null, string memo = null, bool? cleared = null)
            : base(date, memo: memo, cleared: cleared)
        {
            _accountId = account?.Id ?? -1;
            _payeeId = payee?.Id ?? -1;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TransIncBase(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
            : base(date, id, memo, cleared)
        {
            _accountId = accountId;
            _payeeId = payeeId;
        }
    }
}
