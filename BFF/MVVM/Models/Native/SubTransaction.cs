using System;
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
        public SubTransaction(IRepository<SubTransaction> repository, 
                              IParentTransaction parent = null, 
                              ICategory category = null, 
                              long sum = 0L,
                              string memo = null) 
            : base(repository, parent, category, memo, sum) {}

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The Sum of the SubElement</param>
        public SubTransaction(IRepository<SubTransaction> repository, 
                              long id, 
                              long parentId, 
                              long categoryId,
                              string memo, 
                              long sum)
            : base(repository, id, parentId, categoryId, sum, memo) { }
    }
}
