using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IPayee : ICommonProperty {}
    
    public class Payee : CommonProperty<IPayee>, IPayee
    {
        public Payee(
            IRepository<IPayee> repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            long id = -1L, 
            string name = "") : base(repository, rxSchedulerProvider, name: name)
        {
            if (id > 0L) Id = id;
        }
    }
}
