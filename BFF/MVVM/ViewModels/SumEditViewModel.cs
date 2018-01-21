using System;
using BFF.Helper;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    public interface ISumEditViewModel
    {
        Sign SumSign { get; set; }

        long SumAbsolute { get; set; }

        long Sum { get; set; }

        ReactiveCommand ToggleSign { get; }
    }

    public class SumEditViewModel : ObservableObject, ISumEditViewModel
    {

        private Sign _sumSign = Sign.Minus;

        private readonly Func<long> _getSum = () => 0L;

        private readonly Action<long> _setSum = l => {};

        public SumEditViewModel(Func<long> getSum, Action<long> setSum)
        {
            if(getSum != null) _getSum = getSum;
            if(setSum != null)_setSum = setSum;
            ToggleSign = new ReactiveCommand();
            ToggleSign.Subscribe(_ => SumSign = SumSign == Sign.Plus ? Sign.Minus : Sign.Plus);
        }

        public Sign SumSign
        {
            get => Sum == 0 ? _sumSign : Sum > 0 ? Sign.Plus : Sign.Minus;
            set
            {
                if (value == Sign.Plus && Sum < 0 || value == Sign.Minus && Sum > 0)
                    Sum *= -1;
                _sumSign = value;
                OnPropertyChanged();
            }
        }

        public long SumAbsolute
        {
            get => Math.Abs(Sum);
            set
            {
                Sum = (SumSign == Sign.Plus ? 1 : -1) * value;
                OnPropertyChanged();
            }
        }

        public long Sum
        {
            get => _getSum();
            set => _setSum(value);
        }

        public ReactiveCommand ToggleSign { get; }
    }
}
