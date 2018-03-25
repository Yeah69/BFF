using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public interface IEditPayeesViewModel
    {
        ReadOnlyObservableCollection<IPayeeViewModel> All { get; }

        INewPayeeViewModel NewPayeeViewModel { get; }
    }

    public class EditPayeesViewModel : ObservableObject, IEditPayeesViewModel, IOncePerBackend
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
