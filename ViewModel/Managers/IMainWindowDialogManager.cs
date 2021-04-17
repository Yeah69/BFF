using BFF.ViewModel.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace BFF.ViewModel.Managers
{
    public interface IMainWindowDialogManager
    {
        Task<T> ShowDialogFor<T>(IMainWindowDialogContentViewModel<T> contentViewModel);
    }
}