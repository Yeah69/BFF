using System;
using System.Reactive;
using System.Reactive.Linq;

namespace BFF.Core.Extensions
{
    public static class IObservableExtensions
    {
        public static IObservable<Unit> ToUnitObservable<T>(this IObservable<T> observable) =>
            observable.Select(_ => Unit.Default);
    }
}
