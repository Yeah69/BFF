namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHavePayeeViewModel
    {
        IPayeeViewModel Payee { get; set; }

        INewPayeeViewModel NewPayeeViewModel { get; }
    }
}
