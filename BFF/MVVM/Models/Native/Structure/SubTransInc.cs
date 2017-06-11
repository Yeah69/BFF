using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ISubTransInc : ITitLike, IHaveCategory
    {
        /// <summary>
        /// Id of the Parent
        /// </summary>
        long ParentId { get; set; }

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
        private long _parentId;
        private long _categoryId;

        /// <summary>
        /// Id of the Parent
        /// </summary>
        public long ParentId
        {
            get => _parentId;
            set
            {
                if(_parentId == value) return;
                _parentId = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of the Category
        /// </summary>
        public long CategoryId
        {
            get => _categoryId;
            set
            {
                if(_categoryId == value) return;
                _categoryId = value;
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
        protected SubTransInc(IRepository<T> repository, 
                              IParentTransInc parent = null,
                              ICategory category = null,
                              string memo = null,
                              long sum = 0L) : base(repository, memo: memo)
        {
            _parentId = parent?.Id ?? -1;
            _categoryId = category?.Id ?? -1;
            _sum = sum;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTransInc(IRepository<T> repository, long id, long parentId, long categoryId, long sum, string memo) 
            : base(repository, id, memo)
        {
            _parentId = parentId;
            _categoryId = categoryId;
            _sum = sum;
        }
    }
}
