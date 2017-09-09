using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ISubTransInc : ITitLike, IHaveCategory
    {
        /// <summary>
        /// The amount of money, which was payed or received
        /// </summary>
        long Sum { get; set; }
    }

    /// <summary>
    /// Base class for all SubElement classes, which are used by TitNoTransfer classes
    /// </summary>
    public abstract class SubTransInc<T> : TitLike<T>, ISubTransInc where T : class, ISubTransInc
    {
        private ICategory _category;

        /// <summary>
        /// Id of the Category
        /// </summary>
        public ICategory Category
        {
            get => _category;
            set
            {
                if (value == null)
                {
                    OnPropertyChanged();
                    return;
                }
                if (_category == value) return;
                _category = value;
                Update();
                OnPropertyChanged();
            }
        }

        private long _sum;

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
        /// Initializes the SubTransInc-parts of the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTransInc(
            IRepository<T> repository, 
            long id,
            ICategory category = null,
            string memo = null,
            long sum = 0L) : base(repository, id, memo)
        {
            _category = category;
            _sum = sum;
        }
    }
}
