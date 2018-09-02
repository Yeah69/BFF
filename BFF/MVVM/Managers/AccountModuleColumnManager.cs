using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.Properties;
using Reactive.Bindings;

namespace BFF.MVVM.Managers
{
    public interface ITransDataGridColumnManager : INotifyPropertyChanged
    {
        IReactiveProperty<bool> ShowFlags { get; }
        IReactiveProperty<bool> ShowCheckNumbers { get; }

        bool NeverShowEditHeaders { get; set; }
    }

    public class TransDataGridColumnManager : ObservableObject, ITransDataGridColumnManager, IOncePerApplication, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private bool _neverShowEditHeaders;

        public TransDataGridColumnManager()
        {
            ShowFlags = new ReactiveProperty<bool>(Settings.Default.ShowFlags, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            ShowFlags.Subscribe(v =>
            {
                Settings.Default.ShowFlags = v;
                Settings.Default.Save();
            }).AddHere(_compositeDisposable);

            ShowCheckNumbers = new ReactiveProperty<bool>(Settings.Default.ShowCheckNumbers, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            ShowCheckNumbers.Subscribe(v =>
            {
                Settings.Default.ShowCheckNumbers = v;
                Settings.Default.Save();
            }).AddHere(_compositeDisposable);

            _neverShowEditHeaders = Settings.Default.NeverShowEditHeaders;
        }

        public IReactiveProperty<bool> ShowFlags { get; }
        public IReactiveProperty<bool> ShowCheckNumbers { get; }

        public bool NeverShowEditHeaders
        {
            get => _neverShowEditHeaders;
            set
            {
                if (_neverShowEditHeaders == value) return;
                _neverShowEditHeaders = value;
                OnPropertyChanged();

                Settings.Default.NeverShowEditHeaders = value;
                Settings.Default.Save();
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
