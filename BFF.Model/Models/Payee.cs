using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface IPayee : ICommonProperty
    {
        Task MergeToAsync(IPayee payee);
    }

    public abstract class Payee : CommonProperty, IPayee
    {

        public Payee(
            IRxSchedulerProvider rxSchedulerProvider, 
            string name) : base(rxSchedulerProvider, name)
        {
        }

        public abstract Task MergeToAsync(IPayee payee);
    }
}
