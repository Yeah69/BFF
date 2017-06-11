using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IPayee : ICommonProperty {}

    /// <summary>
    /// Someone to whom was payeed or who payeed himself
    /// </summary>
    public class Payee : CommonProperty<Payee>, IPayee
    {
        /// <summary>
        /// Initializing the object
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="name">Name of the Payee</param>
        public Payee(IRepository<Payee> repository, long id = -1L, string name = null) : base(repository, name)
        {
            if (id > 0L) Id = id;
        }
    }
}
