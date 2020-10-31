using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.ViewModel.Helper;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;

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
        ICommand OkCommand { get; }
        IRxRelayCommand CancelCommand { get; }

        OkCancelResult Result { get; }
    }

    

    public abstract class OkCancelDialogViewModel : DialogViewModel, IOkCancelDialogViewModel
    {
        protected readonly SerialDisposable OkCommandSerialDisposable = new SerialDisposable();
        
        protected OkCancelDialogViewModel()
        {
            OkCommandSerialDisposable.CompositeDisposalWith(CompositeDisposable);
            var okCommandCompositeDisposable = new CompositeDisposable().SerializeDisposalWith(OkCommandSerialDisposable);
            var okCommand = RxCommand.CanAlwaysExecute().CompositeDisposalWith(okCommandCompositeDisposable);
            okCommand.Observe.Subscribe(_ => OnOk()).CompositeDisposalWith(okCommandCompositeDisposable);

            this.OkCommand = okCommand;

            CancelCommand = new RxRelayCommand(() =>
                {
                    IsOpen.Value = false;
                    CompositeDisposable.Dispose();
                })
                .CompositeDisposalWith(CompositeDisposable);
        }

        public ICommand OkCommand { get; protected set; }
        public IRxRelayCommand CancelCommand { get; }
        public OkCancelResult Result { get; private set; } = OkCancelResult.None;

        protected void OnOk()
        {
            Result = OkCancelResult.Ok;
            IsOpen.Value = false;
            CompositeDisposable.Dispose();
        }
    }
}
