using System.Reactive.Disposables;
using MrMeeseeks.Reactive.Extensions;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IDialogViewModel
    {
        IReactiveProperty IsOpen { get; }
    }

    public abstract class DialogViewModel : ViewModelBase, IDialogViewModel
    {
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public DialogViewModel()
        {
            IsOpen = new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged)
                .CompositeDisposalWith(CompositeDisposable);
        }
        public IReactiveProperty IsOpen { get; }
    }
}
