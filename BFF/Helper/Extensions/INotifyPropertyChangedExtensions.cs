using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;

namespace BFF.Helper.Extensions
{
    public static class NotifyPropertyChangedExtensions
    {
        public static IObservable<Unit> ObservePropertyChanges(
            this INotifyPropertyChanged source,
            string propertyName)
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler => handler.Invoke,
                    h => source.PropertyChanged += h,
                    h => source.PropertyChanged -= h)
                .Where(e => e.EventArgs.PropertyName == propertyName)
                .Select(_ => Unit.Default);
        }
    }
}
