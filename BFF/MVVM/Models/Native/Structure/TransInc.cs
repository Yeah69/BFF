using System;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransInc : ITransIncBase, IHaveCategory
    {
        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        long Sum { get; set; }
    }

    public abstract class TransInc : TransIncBase, ITransInc
    {
        private long _categoryId;
        private long _sum;

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
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public long Sum
        {
            get { return _sum; }
            set
            {
                _sum = value;
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
        protected TransInc(DateTime date, IAccount account = null, IPayee payee = null, ICategory category = null, string memo = null, 
            long sum = 0, bool? cleared = null) 
            : base(date, account, payee, memo, cleared)
        {
            _categoryId = category?.Id ?? -1;
            _sum = sum;
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
        protected TransInc(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared) 
            : base(id, accountId, date, payeeId, memo, cleared)
        {
            _categoryId = categoryId;
            _sum = sum;
        }
    }
}
