using System.Collections.ObjectModel;
using BFF.Core.IoCMarkerInterfaces;
using BFF.DB;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public interface IEditFlagsViewModel
    {
        ReadOnlyObservableCollection<IFlagViewModel> All { get; }

        INewFlagViewModel NewFlagViewModel { get; }
    }

    public class EditFlagsViewModel : ViewModelBase, IEditFlagsViewModel, IOncePerBackend
    {
        public INewFlagViewModel NewFlagViewModel { get; }
        public ReadOnlyObservableCollection<IFlagViewModel> All { get; }

        public EditFlagsViewModel(
            IFlagViewModelService service,
            INewFlagViewModel newFlagViewModel)
        {
            NewFlagViewModel = newFlagViewModel;
            All = service.All.ToReadOnlyObservableCollection();
        }
    }
}
