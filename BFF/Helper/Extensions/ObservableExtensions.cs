using System;
using System.Threading.Tasks;
using BFF.MVVM;

namespace BFF.Helper.Extensions
{
    public static class ObservableExtensions
    {
        public static IRxRelayCommand ToRxRelayCommand(this IObservable<bool> @this, Action execute)
            => new RxRelayCommand(execute, @this);

        public static IRxRelayCommand ToRxRelayCommand(this IObservable<bool> @this, Action execute, bool initialCanExecute)
            => new RxRelayCommand(execute, @this, initialCanExecute);

        public static IRxRelayCommand ToRxRelayCommand(this IObservable<bool> @this, Func<Task> execute)
            => new AsyncRxRelayCommand(execute, @this);

        public static IRxRelayCommand ToRxRelayCommand(this IObservable<bool> @this, Func<Task> execute, bool initialCanExecute)
            => new AsyncRxRelayCommand(execute, @this, initialCanExecute);
    }
}
