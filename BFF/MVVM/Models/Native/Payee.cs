using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IPayee : ICommonProperty {}

    /// <summary>
    /// Someone to whom was payed or who payed himself
    /// </summary>
    public class Payee : CommonProperty<IPayee>, IPayee
    {
        /// <summary>
        /// Initializing the object
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="name">Name of the Payee</param>
        public Payee(IRepository<IPayee> repository, long id = -1L, string name = "") : base(repository, name: name)
        {
            if (id > 0L) Id = id;
        }
    }
}
