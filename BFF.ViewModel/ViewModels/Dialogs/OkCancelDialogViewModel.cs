using BFF.ViewModel.Helper;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public enum OkCancelResult
    {   
        None,
        Ok,
        Cancel
    }

    public interface IOkCancelDialogViewModel : IDialogViewModel
    {
        IRxRelayCommand OkCommand { get; }
        IRxRelayCommand CancelCommand { get; }

        OkCancelResult Result { get; }
    }

    

    public abstract class OkCancelDialogViewModel : DialogViewModel, IOkCancelDialogViewModel
    {
        public OkCancelDialogViewModel()
        {

            OkCommand = new RxRelayCommand(() =>
                {
                    Result = OkCancelResult.Ok;
                    IsOpen.Value = false;
                    CompositeDisposable.Dispose();
                })
                .AddForDisposalTo(CompositeDisposable);

            CancelCommand = new RxRelayCommand(() =>
                {
                    IsOpen.Value = false;
                    CompositeDisposable.Dispose();
                })
                .AddForDisposalTo(CompositeDisposable);
        }

        public IRxRelayCommand OkCommand { get; }
        public IRxRelayCommand CancelCommand { get; }
        public OkCancelResult Result { get; private set; } = OkCancelResult.None;
    }
}
