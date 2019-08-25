using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using BFF.Core.Helper;
using BFF.Core.IoC;
using MrMeeseeks.Extensions;
using Reactive.Bindings;

namespace BFF.ViewModel.Managers
{
    public interface ITransDataGridColumnManager : INotifyPropertyChanged
    {
        IReactiveProperty<bool> ShowFlags { get; }
        IReactiveProperty<bool> ShowCheckNumbers { get; }

        bool NeverShowEditHeaders { get; set; }
    }

    internal class TransDataGridColumnManager : ObservableObject, ITransDataGridColumnManager, IOncePerApplication, IDisposable
    {
        private readonly IBffSettings _bffSettings;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private bool _neverShowEditHeaders;

        public TransDataGridColumnManager(IBffSettings bffSettings)
        {
            _bffSettings = bffSettings;
            ShowFlags = new ReactiveProperty<bool>(_bffSettings.ShowFlags, ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(_compositeDisposable);

            ShowFlags.Subscribe(v =>
            {
                _bffSettings.ShowFlags = v;
            }).AddForDisposalTo(_compositeDisposable);

            ShowCheckNumbers = new ReactiveProperty<bool>(_bffSettings.ShowCheckNumbers, ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(_compositeDisposable);

            ShowCheckNumbers.Subscribe(v =>
            {
                _bffSettings.ShowCheckNumbers = v;
            }).AddForDisposalTo(_compositeDisposable);

            _neverShowEditHeaders = _bffSettings.NeverShowEditHeaders;
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

                _bffSettings.NeverShowEditHeaders = value;
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
