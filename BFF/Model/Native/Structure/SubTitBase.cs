using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    /// <summary>
    /// Base class for all SubElement classes, which are used by TitNoTransfer classes
    /// </summary>
    public abstract class SubTitBase : TitLike
    {
        private Category _category;

        /// <summary>
        /// This instance is a SubElement of the Parent
        /// </summary>
        [Write(false)]
        public abstract TitNoTransfer Parent{ get; set; }

        /// <summary>
        /// Id of the Parent
        /// </summary>
        public long ParentId
        {
            get { return Parent?.Id ?? -1L; }
            set { }
        }

        /// <summary>
        /// Category of the SubElement
        /// </summary>
        [Write(false)]
        public virtual Category Category
        {
            get { return _category; }
            set
            {
                _category = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of the Category
        /// </summary>
        public long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category = Database?.GetCategory(value); }
        }

        /// <summary>
        /// Initializes the SubTitBase-parts of the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTitBase(Category category = null, string memo = null, long sum = 0L) : base(memo: memo, sum: sum)
        {
            ConstrDbLock = true;

            _category = category ?? _category;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTitBase(long id, long parentId, long categoryId, long sum, string memo) : base(id, memo, sum)
        {
            ConstrDbLock = true;

            ParentId = parentId;
            CategoryId = categoryId;

            ConstrDbLock = false;
        }
    }
}
