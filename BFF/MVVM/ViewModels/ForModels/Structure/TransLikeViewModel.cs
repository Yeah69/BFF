﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransLikeViewModel : IDataModelViewModel
    {
        string Memo { get; set; }
        
        IReactiveProperty<long> Sum { get; }

        Sign SumSign { get; }

        ISumEditViewModel SumEdit { get; }

        long SumAbsolute { get; }
        
        IRxRelayCommand ToggleSign { get; }

        IObservable<Unit> RemoveRequests { get; }

        IRxRelayCommand RemoveCommand { get; }

        IAccountBaseViewModel Owner { get; }
    }
    
    public abstract class TransLikeViewModel : DataModelViewModel, ITransLikeViewModel, ITransientViewModel
    {
        private readonly ITransLike _transLike;
        private readonly Subject<Unit> _removeRequestSubject = new Subject<Unit>();

        public virtual string Memo
        {
            get => _transLike.Memo;
            set => _transLike.Memo = value;
        }
        
        public abstract IReactiveProperty<long> Sum { get; }

        private Sign _sumSign = Sign.Minus;

        public Sign SumSign
        {
            get => Sum.Value == 0 ? _sumSign : Sum.Value > 0 ? Sign.Plus : Sign.Minus;
            set
            {
                if (value == Sign.Plus && Sum.Value < 0 || value == Sign.Minus && Sum.Value > 0)
                    Sum.Value *= -1;
                _sumSign = value;
                OnPropertyChanged();
            }
        }

        public long SumAbsolute
        {
            get => Math.Abs(Sum.Value);
            set
            {
                Sum.Value = (SumSign == Sign.Plus ? 1 : -1) * value;
                OnPropertyChanged();
            }
        }

        public abstract ISumEditViewModel SumEdit { get; }
        
        protected TransLikeViewModel(
            ITransLike transLike,
            IRxSchedulerProvider rxSchedulerProvider,
            IAccountBaseViewModel owner) 
            : base(transLike, rxSchedulerProvider)
        {
            _transLike = transLike;
            Owner = owner;
            _removeRequestSubject.AddTo(CompositeDisposable);

            transLike
                .ObservePropertyChanges(nameof(transLike.Memo))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Memo)))
                .AddTo(CompositeDisposable);


            ToggleSign = new RxRelayCommand(() => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus).AddTo(CompositeDisposable);

            RemoveCommand = new RxRelayCommand(() => _removeRequestSubject.OnNext(Unit.Default)).AddTo(CompositeDisposable);
        }

        public IRxRelayCommand ToggleSign { get; }
        public IObservable<Unit> RemoveRequests => _removeRequestSubject.AsObservable();
        public IRxRelayCommand RemoveCommand { get; }
        public IAccountBaseViewModel Owner { get; }
    }
}
