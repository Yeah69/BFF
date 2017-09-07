using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransIncBase : ITitBase
    {
        /// <summary>
        /// Id of Account
        /// </summary>
        IAccount Account { get; set; }

        /// <summary>
        /// Id of Payee
        /// </summary>
        IPayee Payee { get; set; }
    }

    /// <summary>
    /// Base of all Tit-classes except Transfer (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TransIncBase<T> : TitBase<T>, ITransIncBase where T : class, ITransIncBase
    {
        private IAccount _account;
        private IPayee _payee;

        /// <summary>
        /// Id of Account
        /// </summary>
        public IAccount Account
        {
            get => _account;
            set
            {
                if(_account == value) return;
                _account = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Payee
        /// </summary>
        public IPayee Payee
        {
            get => _payee;
            set
            {
                if(_payee == value) return;
                _payee = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payed or who payed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TransIncBase(IRepository<T> repository,
                               long id,
                               DateTime date,
                               IAccount account = null, 
                               IPayee payee = null, 
                               string memo = null, 
                               bool? cleared = null)
            : base(repository, date, id, memo, cleared)
        {
            _account = account;
            _payee = payee;
        }
    }
}
