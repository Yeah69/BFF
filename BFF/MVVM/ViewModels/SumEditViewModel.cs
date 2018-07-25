using System;
using System.Reactive.Disposables;
using BFF.Helper;
using BFF.Helper.Extensions;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    public interface ISumEditViewModel : IDisposable
    {
        Sign SumSign { get; set; }

        long SumAbsolute { get; set; }

        IReactiveProperty<long> Sum { get; }

        IRxRelayCommand ToggleSign { get; }
    }

    public class SumEditViewModel : ViewModelBase, ISumEditViewModel
    {

        private Sign _sumSign = Sign.Minus;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public SumEditViewModel(IReactiveProperty<long> sum)
        {
            Sum = sum;
            ToggleSign = new RxRelayCommand(() => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus)
                .AddHere(_compositeDisposable);
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
