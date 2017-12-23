using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ITransaction : ITransactionBase, IHaveCategory
    {
        /// <summary>
        /// The amount of money, which was payed or received
        /// </summary>
        long Sum { get; set; }
    }

    /// <summary>
    /// The Transaction documents payment to or from externals
    /// </summary>
    public class Transaction : TransactionBase<ITransaction>, ITransaction
    {
        private ICategoryBase _category;
        private long _sum;

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payed or who payed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payed or received</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transaction(
            IRepository<ITransaction> repository,
            long id,
            string checkNumber,
            DateTime date, 
            IAccount account = null, 
            IPayee payee = null, 
            ICategoryBase category = null,
            string memo = null, 
            long sum = 0L, 
            bool? cleared = null)
            : base(repository, id, checkNumber, date, account, payee, memo, cleared)
        {
            _category = category;
            _sum = sum;
        }

        /// <summary>
        /// Id of Category
        /// </summary>
        public ICategoryBase Category
        {
            get => _category;
            set
            {
                if (value == null)
                {
                    OnPropertyChanged();
                    return;
                }
                if(_category == value) return;
                _category = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money, which was payed or received
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
    }
}
