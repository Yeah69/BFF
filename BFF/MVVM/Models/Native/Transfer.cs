using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ITransfer : ITitBase {
        /// <summary>
        /// Id of FromAccount
        /// </summary>
        IAccount FromAccount { get; set; }

        /// <summary>
        /// Id of ToAccount
        /// </summary>
        IAccount ToAccount { get; set; }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        long Sum { get; set; }
    }

    /// <summary>
    /// A Transfer is basically a Transaction from one owned Account to another owned Account
    /// </summary>
    public class Transfer : TitBase<Transfer>, ITransfer
    {
        private IAccount _fromAccount;
        private IAccount _toAccount;
        private long _sum;

        /// <summary>
        /// Id of FromAccount
        /// </summary>
        public IAccount FromAccount
        {
            get => _fromAccount;
            set
            {
                if(_fromAccount == value) return;
                _fromAccount = value; 
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of ToAccount
        /// </summary>
        public IAccount ToAccount
        {
            get => _toAccount;
            set
            {
                if(_toAccount == value) return;
                _toAccount = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public long Sum
        {
            get => _sum;
            set
            {
                if(_sum == value) return;
                _sum = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="fromAccount">The Sum is transfered from this Account</param>
        /// <param name="toAccount">The Sum is transfered to this Account</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The transfered Sum</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transfer(
            IRepository<Transfer> repository,
            long id,
            DateTime date,
            IAccount fromAccount = null,
            IAccount toAccount = null,
            string memo = null,
            long sum = 0L,
            bool? cleared = null)
            : base(repository, date, id, memo, cleared)
        {
            _fromAccount = fromAccount;
            _toAccount = toAccount;
            _sum = sum;
        }
    }
}
