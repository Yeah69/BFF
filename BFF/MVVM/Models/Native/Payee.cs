using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IPayee : ICommonProperty {}
    
    public class Payee : CommonProperty<IPayee>, IPayee
    {
        public Payee(IRepository<IPayee> repository, long id = -1L, string name = "") : base(repository, name: name)
        {
            if (id > 0L) Id = id;
        }
    }
}
