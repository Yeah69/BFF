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
        public static IObservable<TProperty> ObservePropertyChanges<TSource, TProperty>(
            this TSource source,
            Expression<Func<TSource, TProperty>> selector) 
            where TSource : INotifyPropertyChanged
        {
            if (!(selector.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{selector}' refers to a method, not a property.");

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{selector}' refers to a field, not a property.");

            if (!propInfo.ReflectedType?.IsAssignableFrom(typeof(TSource)) ?? true)
                throw new ArgumentException(
                    $"Expression '{selector}' refers to a property that is not from type {typeof(TSource)}.");

            var property = selector.Compile();

            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler => handler.Invoke,
                    h => source.PropertyChanged += h,
                    h => source.PropertyChanged -= h)
                .Where(e => e.EventArgs.PropertyName == propInfo.Name)
                .Select(_ => property(source));
        }

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
