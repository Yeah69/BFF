using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BFF.ViewModel.Extensions
{
    public static class IDisposableRxCommandExtensions
    {
        public static ICommand StandardCase(
            this IDisposableRxCommand disposableRxCommand,
            CompositeDisposable compositeDisposable,
            Action action)
        {
            disposableRxCommand.CompositeDisposalWith(compositeDisposable);
            disposableRxCommand
                .Observe
                .Subscribe(_ => action())
                .CompositeDisposalWith(compositeDisposable);
            return disposableRxCommand;
        }
        
        public static ICommand StandardCaseAsync(
            this IDisposableRxCommand disposableRxCommand,
            CompositeDisposable compositeDisposable,
            Func<Task> asyncAction)
        {
            disposableRxCommand.CompositeDisposalWith(compositeDisposable);
            disposableRxCommand
                .Observe
                .SelectMany(async _ =>
                {
                    await asyncAction().ConfigureAwait(false);
                    return Unit.Default;
                })
                .SubscribeUnit(() => { })
                .CompositeDisposalWith(compositeDisposable);
            return disposableRxCommand;
        }
    }
}