using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IPayeeViewModel : ICommonPropertyViewModel {}

    public class PayeeViewModel : CommonPropertyViewModel, IPayeeViewModel
    {
        public PayeeViewModel(
            IPayee payee,
            IRxSchedulerProvider rxSchedulerProvider) : base(payee, rxSchedulerProvider)
        {
        }
    }
}