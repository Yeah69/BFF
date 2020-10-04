using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.Extensions
{
    public static class ReactivePropertyExtensions
    {
        public static ReadOnlyReactiveProperty<TProperty> ToReadOnlyReactivePropertyAsSynchronized<TSubject, TProperty>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged =>
            ToReadOnlyReactivePropertyAsSynchronized(subject, propertySelector, ReactivePropertyScheduler.Default, mode, ignoreValidationErrorValue);

        public static ReadOnlyReactiveProperty<TProperty> ToReadOnlyReactivePropertyAsSynchronized<TSubject, TProperty>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            IScheduler raiseEventScheduler,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged
        {
            var result = subject.ObserveProperty(propertySelector)
                .ToReadOnlyReactiveProperty(eventScheduler: raiseEventScheduler, mode: mode);

            return result;
        }
        
        private static IObservable<TProperty> ObserveProperty<TSubject, TProperty>(
            this TSubject subject, string propertyName, Func<TProperty> getter,
            bool isPushCurrentValueAtFirst = true)
            where TSubject : INotifyPropertyChanged
        {
            var isFirst = true;

            var result = Observable.Defer(() =>
            {
                var flag = isFirst;
                isFirst = false;

                var q = subject.PropertyChangedAsObservable()
                    .Where(e => e.PropertyName == propertyName)
                    .Select(_ => getter());
                return isPushCurrentValueAtFirst && flag ? q.StartWith(getter()) : q;
            });
            return result;
        }
        
        public static ReactiveProperty<TProperty> ToReactivePropertyAsSynchronized<TSubject, TProperty>(
            this TSubject subject,
            string propertyName,
            Func<TProperty> getter,
            Action<TProperty> setter,
            IScheduler raiseEventScheduler,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged
        {
            var result = subject.ObserveProperty(propertyName, getter)
                .ToReactiveProperty(raiseEventScheduler, mode: mode);
            result
                .Where(_ => !ignoreValidationErrorValue || !result.HasErrors)
                .Subscribe(setter);

            return result;
        }
    }
}
