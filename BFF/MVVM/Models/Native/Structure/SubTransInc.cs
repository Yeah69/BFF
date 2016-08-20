namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Base class for all SubElement classes, which are used by TitNoTransfer classes
    /// </summary>
    public abstract class SubTransInc : TitLike, IHaveCategory
    {
        private long _parentId;
        private long _categoryId;

        /// <summary>
        /// Id of the Parent
        /// </summary>
        public long ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of the Category
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

        private long _sum;

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
        /// Initializes the SubTransInc-parts of the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTransInc(Category category = null, string memo = null, long sum = 0L) : base(memo: memo)
        {
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
        protected SubTransInc(long id, long parentId, long categoryId, long sum, string memo) : base(id, memo)
        {
            _parentId = parentId;
            _categoryId = categoryId;
            _sum = sum;
        }
    }
}
