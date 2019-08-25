﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels.Utility
{
    public interface ILazyElementsViewModelBase<out T> : INotifyPropertyChanged, IDisposable
    {
        IEnumerable<T> LazyElements { get; }

        bool OpenFlag { get; set; }

        IRxRelayCommand LoadLazyElements { get; }

        IRxRelayCommand ClearLazyElements { get; }
    }

    internal class LazyElementsViewModelBase<T> : ViewModelBase, ILazyElementsViewModelBase<T>
    {
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();
        private bool _openFlag;

        public LazyElementsViewModelBase(Func<Task<IEnumerable<T>>> elementsFactoryAsync)
        {
            LazyElements = Enumerable.Empty<T>();

            LoadLazyElements = new AsyncRxRelayCommand(async () =>
                {
                    LazyElements = await elementsFactoryAsync();
                    OnPropertyChanged(nameof(LazyElements));
                    if (LazyElements.Any().Not())
                        OpenFlag = false;
                })
                .AddForDisposalTo(CompositeDisposable);

            ClearLazyElements = new RxRelayCommand(() =>
                {
                    LazyElements = Enumerable.Empty<T>();
                    OnPropertyChanged(nameof(LazyElements));
                })
                .AddForDisposalTo(CompositeDisposable);
        }

        public IEnumerable<T> LazyElements { get; private set; }

        public bool OpenFlag
        {
            get => _openFlag;
            set
            {
                if (_openFlag == value) return;
                _openFlag = value;
                OnPropertyChanged();
            }
        }

        public IRxRelayCommand LoadLazyElements { get; }
        public IRxRelayCommand ClearLazyElements { get; }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }

    public interface ILazyTransLikeViewModels : ILazyElementsViewModelBase<ITransLikeViewModel>
    {
    }

    internal class LazyTransLikeViewModels : LazyElementsViewModelBase<ITransLikeViewModel>, ILazyTransLikeViewModels
    {
        public LazyTransLikeViewModels(Func<Task<IEnumerable<ITransLikeViewModel>>> elementsFactoryAsync) : base(elementsFactoryAsync)
        {
        }
    }
}
