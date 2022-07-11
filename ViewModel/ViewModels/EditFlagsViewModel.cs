using System.Collections.ObjectModel;
using BFF.Core.IoC;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface IEditFlagsViewModel
    {
        ReadOnlyObservableCollection<IFlagViewModel> All { get; }

        INewFlagViewModel NewFlagViewModel { get; }
    }

    public class EditFlagsViewModel : ViewModelBase, IEditFlagsViewModel, IScopeInstance
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
