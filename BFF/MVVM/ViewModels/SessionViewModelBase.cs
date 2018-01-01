using System;
using System.Globalization;
using System.Reactive.Disposables;
using System.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels
{
    public abstract class SessionViewModelBase : ObservableObject, IDisposable
    {
        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();

        protected SessionViewModelBase()
        {
            IsOpen = new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            IsOpen.Subscribe(OnIsOpenChanged).AddTo(CompositeDisposable);
        }

        protected abstract CultureInfo CreateCustomCulture();

        protected abstract void SaveCultures();

        protected abstract void OnIsOpenChanged(bool isOpen);

        public IReactiveProperty<bool> IsOpen { get; }

        public void ManageCultures()
        {
            CultureInfo customCulture = CreateCustomCulture();
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = customCulture;
            Thread.CurrentThread.CurrentCulture = customCulture;
            Thread.CurrentThread.CurrentUICulture = customCulture;
            SaveCultures();
            Messenger.Default.Send(CultureMessage.Refresh);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CompositeDisposable?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
