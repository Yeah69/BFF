using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ISubTransInc : ITitLike, IHaveCategory
    {
        /// <summary>
        /// Id of the Parent
        /// </summary>
        IParentTransInc Parent { get; set; }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        long Sum { get; set; }
    }

    /// <summary>
    /// Base class for all SubElement classes, which are used by TitNoTransfer classes
    /// </summary>
    public abstract class SubTransInc<T> : TitLike<T>, ISubTransInc where T : class, ISubTransInc
    {
        private IParentTransInc _parent;
        private ICategory _category;

        /// <summary>
        /// Id of the Parent
        /// </summary>
        public IParentTransInc Parent
        {
            get => _parent;
            set
            {
                if(_parent == value) return;
                _parent = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of the Category
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

        private long _sum;

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
        /// Initializes the SubTransInc-parts of the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTransInc(
            IRepository<T> repository, 
            long id,
            IParentTransInc parent = null,
            ICategory category = null,
            string memo = null,
            long sum = 0L) : base(repository, id, memo)
        {
            _parent = parent;
            _category = category;
            _sum = sum;
        }
    }
}
