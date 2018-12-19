using System;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public abstract class SessionViewModelBase : ViewModelBase, IDisposable
    {
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected SessionViewModelBase()
        {
            IsOpen = new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            IsOpen.Subscribe(OnIsOpenChanged).AddTo(CompositeDisposable);
        }

        protected abstract void OnIsOpenChanged(bool isOpen);

        public IReactiveProperty<bool> IsOpen { get; }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
