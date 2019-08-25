namespace BFF.ViewModel.ViewModels
{
    public interface IEmptyViewModel
    {
    }

    internal class EmptyContentViewModel : SessionViewModelBase, IEmptyViewModel
    {
        protected override void OnIsOpenChanged(bool isOpen)
        {
            
        }
    }
}
