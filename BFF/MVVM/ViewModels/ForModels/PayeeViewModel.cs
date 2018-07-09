using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IPayeeViewModel : ICommonPropertyViewModel
    {
        IRxRelayCommand<IPayeeViewModel> MergeTo { get; }
    }

    public class PayeeViewModel : CommonPropertyViewModel, IPayeeViewModel
    {
        private readonly IPayee _payee;

        public PayeeViewModel(
            IPayee payee,
            IRxSchedulerProvider rxSchedulerProvider) : base(payee, rxSchedulerProvider)
        {
            _payee = payee;

            MergeTo = new RxRelayCommand<IPayeeViewModel>(cvm =>
            {
                if (cvm is PayeeViewModel payeeViewModel)
                {
                    payee.MergeTo(payeeViewModel._payee);
                }
            });
        }

        public IRxRelayCommand<IPayeeViewModel> MergeTo { get; }
    }
}