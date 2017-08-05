using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubTransaction : ISubTransInc {}

    /// <summary>
    /// A SubElement of a Transaction
    /// </summary>
    public class SubTransaction : SubTransInc<SubTransaction>, ISubTransaction
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubTransaction(
            IRepository<SubTransaction> repository,
            long id,
            IParentTransaction parent = null, 
            ICategory category = null,
            string memo = null,
            long sum = 0L) 
            : base(repository, id, parent, category, memo, sum) {}
    }
}
