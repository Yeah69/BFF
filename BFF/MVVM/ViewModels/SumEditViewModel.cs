using System;
using BFF.Helper;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    public interface ISumEditViewModel
    {
        Sign SumSign { get; set; }

        long SumAbsolute { get; set; }

        IReactiveProperty<long> Sum { get; }

        ReactiveCommand ToggleSign { get; }
    }

    public class SumEditViewModel : ObservableObject, ISumEditViewModel
    {

        private Sign _sumSign = Sign.Minus;

        public SumEditViewModel(IReactiveProperty<long> sum)
        {
            Sum = sum;
            ToggleSign = new ReactiveCommand();
            ToggleSign.Subscribe(_ => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus);
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

        public ReactiveCommand ToggleSign { get; }
    }
}
