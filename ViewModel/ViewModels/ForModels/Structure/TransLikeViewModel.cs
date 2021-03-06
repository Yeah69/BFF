﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface ITransLikeViewModel : IDataModelViewModel
    {
        string Memo { get; set; }
        
        IReactiveProperty<long> Sum { get; }

        Sign SumSign { get; }

        ISumEditViewModel SumEdit { get; }

        long SumAbsolute { get; }
        
        ICommand ToggleSign { get; }

        IObservable<Unit> RemoveRequests { get; }

        ICommand RemoveCommand { get; }

        IAccountBaseViewModel? Owner { get; }

        void NotifyErrorsIfAny();
    }

    public abstract class TransLikeViewModel : DataModelViewModel, ITransLikeViewModel, ITransient
    {
        private readonly ITransLike _transLike;
        private readonly Subject<Unit> _removeRequestSubject = new();

        public virtual string Memo
        {
            get => _transLike.Memo;
            set => _transLike.Memo = value;
        }
        
        public abstract IReactiveProperty<long> Sum { get; }

        private Sign _sumSign = Sign.Minus;

        public Sign SumSign
        {
            get => Sum?.Value == 0 ? _sumSign : Sum?.Value > 0 ? Sign.Plus : Sign.Minus;
            protected set
            {
                if (value == Sign.Plus && Sum?.Value < 0 || value == Sign.Minus && Sum?.Value > 0)
                    Sum.Value *= -1;
                _sumSign = value;
                OnPropertyChanged();
            }
        }

        public long SumAbsolute
        {
            get => Math.Abs(Sum?.Value ?? 0);
            set
            {
                if (Sum is null) return;
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
                .ObservePropertyChanged(nameof(transLike.Memo))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Memo)))
                .AddTo(CompositeDisposable);

            ToggleSign = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus);
            
            RemoveCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => _removeRequestSubject.OnNext(Unit.Default));
        }

        public ICommand ToggleSign { get; }
        public IObservable<Unit> RemoveRequests => _removeRequestSubject.AsObservable();
        public ICommand RemoveCommand { get; }
        public IAccountBaseViewModel Owner { get; }

        public abstract void NotifyErrorsIfAny();
    }
}
