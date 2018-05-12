using System;
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
        IReactiveProperty<string> Memo { get; }
        
        IReactiveProperty<long> Sum { get; }

        Sign SumSign { get; }

        ISumEditViewModel SumEdit { get; }

        long SumAbsolute { get; }
        
        ReactiveCommand ToggleSign { get; }

        IObservable<Unit> RemoveRequests { get; }

        ReactiveCommand RemoveCommand { get; }
    }
    
    public abstract class TransLikeViewModel : DataModelViewModel, ITransLikeViewModel, ITransientViewModel
    {
        private readonly Subject<Unit> _removeRequestSubject = new Subject<Unit>();
        
        public virtual IReactiveProperty<string> Memo { get; }
        
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
            IRxSchedulerProvider schedulerProvider) 
            : base(transLike)
        {
            _removeRequestSubject.AddTo(CompositeDisposable);
            Memo = transLike.ToReactivePropertyAsSynchronized(
                nameof(transLike.Memo), 
                () => transLike.Memo, 
                m => transLike.Memo = m,
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            ToggleSign = new ReactiveCommand();
            ToggleSign.Subscribe(_ => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus);

            RemoveCommand = new ReactiveCommand();
            RemoveCommand.Subscribe(_ => _removeRequestSubject.OnNext(Unit.Default));
        }

        public ReactiveCommand ToggleSign { get; }
        public IObservable<Unit> RemoveRequests => _removeRequestSubject.AsObservable();
        public ReactiveCommand RemoveCommand { get; }
    }
}
