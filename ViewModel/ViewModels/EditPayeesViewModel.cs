using System.Collections.ObjectModel;
using BFF.Core.IoC;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface IEditPayeesViewModel
    {
        ReadOnlyObservableCollection<IPayeeViewModel> All { get; }

        INewPayeeViewModel NewPayeeViewModel { get; }
    }

    internal class EditPayeesViewModel : ViewModelBase, IEditPayeesViewModel, IOncePerBackend
    {
        public INewPayeeViewModel NewPayeeViewModel { get; }
        public ReadOnlyObservableCollection<IPayeeViewModel> All { get; }

        public EditPayeesViewModel(
            IPayeeViewModelService service,
            INewPayeeViewModel newPayeeViewModel)
        {
            NewPayeeViewModel = newPayeeViewModel;
            All = service.All.ToReadOnlyObservableCollection();
        }
    }
}
