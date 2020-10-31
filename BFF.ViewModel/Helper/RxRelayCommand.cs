﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BFF.ViewModel.Helper
{
    public interface IRxRelayCommand<T> : ICommand, IDisposable
    {
    }

    public interface IRxRelayCommand : IRxRelayCommand<object>
    {
    }

    public class RxRelayCommand<T> : IRxRelayCommand<T>
    {
        protected Action<T>? ExecuteAction;
        private readonly IDisposable _canExecuteSubscription;
        private bool _canExecute;

        protected RxRelayCommand(IObservable<bool> canExecute, bool initialCanExecute = true)
        {
            _canExecute = initialCanExecute;

            _canExecuteSubscription = canExecute
                .Where(b => b != _canExecute)
                .Subscribe(b =>
                {
                    _canExecute = b;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                });
        }

        public RxRelayCommand(Action<T> executeAction, bool initialCanExecute = true) : this(executeAction, Observable.Never<bool>(), initialCanExecute)
        {
        }

        public RxRelayCommand(Action<T> executeAction, IObservable<bool> canExecute, bool initialCanExecute = true) : this(canExecute, initialCanExecute) 
            => ExecuteAction = executeAction;

        public bool CanExecute(object parameter) => _canExecute;

        public void Execute(object parameter) => ExecuteAction?.Invoke((T) parameter);

        public event EventHandler? CanExecuteChanged;

        public void Dispose() => _canExecuteSubscription.Dispose();
    }

    public sealed class RxRelayCommand : RxRelayCommand<object>, IRxRelayCommand
    {
        private RxRelayCommand(IObservable<bool> canExecute, bool initialCanExecute = true) : base(canExecute, initialCanExecute)
        {
        }

        public RxRelayCommand(Action executeAction, bool initialCanExecute = true) : this(Observable.Never<bool>(), initialCanExecute)
            => ExecuteAction = _ => executeAction();

        public RxRelayCommand(Action executeAction, IObservable<bool> canExecute, bool initialCanExecute = true) : this(canExecute, initialCanExecute) 
            => ExecuteAction = _ => executeAction();
    }

    internal class AsyncRxRelayCommand<T> : IRxRelayCommand<T>
    {
        protected Func<T, Task>? ExecuteAction;
        private readonly IDisposable _canExecuteSubscription;
        private bool _canExecute;

        protected AsyncRxRelayCommand(IObservable<bool> canExecute, bool initialCanExecute = true)
        {
            _canExecute = initialCanExecute;

            _canExecuteSubscription = canExecute
                .Where(b => b != _canExecute)
                .Subscribe(b =>
                {
                    _canExecute = b;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                });
        }

        public AsyncRxRelayCommand(Func<T, Task> executeAction, bool initialCanExecute = true) : this(executeAction, Observable.Never<bool>(), initialCanExecute)
        {
        }

        public AsyncRxRelayCommand(Func<T, Task> executeAction, IObservable<bool> canExecute, bool initialCanExecute = true) : this(canExecute, initialCanExecute) 
            => ExecuteAction = executeAction;

        public bool CanExecute(object parameter) => _canExecute;

        public void Execute(object parameter) => ExecuteAction?.Invoke((T) parameter);

        public event EventHandler? CanExecuteChanged;

        public void Dispose() => _canExecuteSubscription.Dispose();
    }

    internal sealed class AsyncRxRelayCommand : AsyncRxRelayCommand<object>, IRxRelayCommand
    {
        private AsyncRxRelayCommand(IObservable<bool> canExecute, bool initialCanExecute = true) : base(canExecute, initialCanExecute)
        {
        }

        public AsyncRxRelayCommand(Func<Task> executeAction, bool initialCanExecute = true) : this(Observable.Never<bool>(), initialCanExecute) 
            => ExecuteAction = _ => executeAction();

        public AsyncRxRelayCommand(Func<Task> executeAction, IObservable<bool> canExecute, bool initialCanExecute = true) : this(canExecute, initialCanExecute)
            => ExecuteAction = _ => executeAction();
    }
}