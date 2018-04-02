﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// A TIT ViewModel Placeholder used for async lazy loaded TIT's.
    /// </summary>
    sealed class TransLikeViewModelPlaceholder : ITransLikeViewModel
    {
        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<string> Memo { get;}

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<long> Sum { get; }

        public Sign SumSign => Sign.Plus;
        public long SumAbsolute => 0L;

        public ReactiveCommand DeleteCommand { get; }
        public ReactiveCommand ToggleSign { get; } = new ReactiveCommand();
        public IObservable<Unit> RemoveRequests => Observable.Never<Unit>();
        public ReactiveCommand RemoveCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<bool> Cleared { get; }

        /// <summary>
        /// Initializes the TransBase-parts of the object
        /// </summary>
        public TransLikeViewModelPlaceholder()
        {
            Memo = new ReactiveProperty<string>("Content is loading…", ReactivePropertyMode.DistinctUntilChanged);
            Sum = new ReactiveProperty<long>(0L, ReactivePropertyMode.DistinctUntilChanged);
            Cleared = new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged);
            DeleteCommand = new ReactiveCommand();
        }

        #region Overrides of TransLikeViewModel

        public void Insert()
        {
            throw new NotImplementedException();
        }

        public bool IsInsertable() => false;

        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
