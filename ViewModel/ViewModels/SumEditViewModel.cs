using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.ViewModel.Helper;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels
{
    public interface ISumEditViewModel : IDisposable
    {
        Sign SumSign { get; set; }

        long SumAbsolute { get; set; }

        IReactiveProperty<long> Sum { get; }

        IRxRelayCommand ToggleSign { get; }
    }

    internal class SumEditViewModel : ViewModelBase, ISumEditViewModel
    {
        private Sign _sumSign = Sign.Minus;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public SumEditViewModel(
            IReactiveProperty<long> sum,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            Sum = sum;
            Sum
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    OnPropertyChanged(nameof(SumAbsolute));
                    OnPropertyChanged(nameof(SumSign));
                })
                .AddTo(_compositeDisposable);
            ToggleSign = new RxRelayCommand(() => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus)
                .CompositeDisposalWith(_compositeDisposable);
        }

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

        public IReactiveProperty<long> Sum { get; }

        public IRxRelayCommand ToggleSign { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
