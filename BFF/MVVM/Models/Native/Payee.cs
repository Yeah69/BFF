using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IPayee : ICommonProperty {}

    /// <summary>
    /// Someone to whom was payeed or who payeed himself
    /// </summary>
    public class Payee : CommonProperty, IPayee
    {
        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Just the Name-property</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Initializing the object
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="name">Name of the Payee</param>
        public Payee(long id = -1L, string name = null) : base(name)
        {
            if (id > 0L) Id = id;
        }

        #region Overrides of ExteriorCrudBase

        public override void Insert(IBffOrm orm)
        {
            orm?.Insert(this);
        }

        public override void Update(IBffOrm orm)
        {
            orm?.Update(this);
        }

        public override void Delete(IBffOrm orm)
        {
            orm?.Delete(this);
        }

        #endregion
    }
}
