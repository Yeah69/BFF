using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// A SubElement of a Transaction
    /// </summary>
    public class SubTransaction : SubTransInc, ISubTransInc
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubTransaction(Category category = null, long sum = 0L, string memo = null) 
            : base(category, memo, sum)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The Sum of the SubElement</param>
        public SubTransaction(long id, long parentId, long categoryId, string memo, long sum) : base(id, parentId, categoryId, sum, memo)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }
    }
}
