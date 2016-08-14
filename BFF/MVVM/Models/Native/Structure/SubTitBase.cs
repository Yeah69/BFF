using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Base class for all SubElement classes, which are used by TitNoTransfer classes
    /// </summary>
    public abstract class SubTitBase : TitLike
    {
        private long _parentId;
        private long _categoryId;
        private long _sum;

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

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public override long Sum
        {
            get { return _sum; }
            set
            {
                _sum = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the SubTitBase-parts of the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTitBase(Category category = null, string memo = null, long sum = 0L) : base(memo: memo)
        {
            ConstrDbLock = true;

            _categoryId = category?.Id ?? -1;
            _sum = sum;

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
        protected SubTitBase(long id, long parentId, long categoryId, long sum, string memo) : base(id, memo)
        {
            ConstrDbLock = true;

            _parentId = parentId;
            _categoryId = categoryId;
            _sum = sum;

            ConstrDbLock = false;
        }
    }
}
