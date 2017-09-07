using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransInc : ITransIncBase, IHaveCategory
    {
        /// <summary>
        /// The amount of money, which was payed or received
        /// </summary>
        long Sum { get; set; }
    }

    public abstract class TransInc<T> : TransIncBase<T>, ITransInc where T : class, ITransInc
    {
        private ICategory _category;
        private long _sum;

        /// <summary>
        /// Id of Category
        /// </summary>
        public ICategory Category
        {
            get => _category;
            set
            {
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
        protected TransInc(IRepository<T> repository,
                           long id,
                           DateTime date, 
                           IAccount account = null,
                           IPayee payee = null, 
                           ICategory category = null,
                           string memo = null, 
                           long sum = 0, 
                           bool? cleared = null) 
            : base(repository, id, date, account, payee, memo, cleared)
        {
            _category = category;
            _sum = sum;
        }
    }
}
