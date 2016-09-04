using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubIncome : ISubTransInc {}

    /// <summary>
    /// A SubElement of an Income
    /// </summary>
    public class SubIncome : SubTransInc, ISubIncome
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubIncome(ICategory category = null, string memo = null, long sum = 0L) 
            : base(category, memo, sum) {}

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The Sum of the SubElement</param>
        public SubIncome(long id, long parentId, long categoryId, string memo, long sum) 
            : base(id, parentId, categoryId, sum, memo) {}
    }
}
