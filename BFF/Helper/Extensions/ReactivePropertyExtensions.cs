using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.Helper.Extensions
{
    public static class ReactivePropertyExtensions
    {

        /// <summary>
        /// <para>Converts NotificationObject's property to ReadOnlyReactiveProperty. Value is one-way synchronized.</para>
        /// <para>PropertyChanged raise on ReactivePropertyScheduler.</para>
        /// </summary>
        /// <param name="propertySelector">Argument is self, Return is target property.</param>
        /// <param name="mode">ReactiveProperty mode.</param>
        /// <param name="ignoreValidationErrorValue">Ignore validation error value.</param>
        public static ReadOnlyReactiveProperty<TProperty> ToReadOnlyReactivePropertyAsSynchronized<TSubject, TProperty>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged =>
            ToReadOnlyReactivePropertyAsSynchronized(subject, propertySelector, ReactivePropertyScheduler.Default, mode, ignoreValidationErrorValue);

        /// <summary>
        /// <para>Converts NotificationObject's property to ReadOnlyReactiveProperty. Value is one-way synchronized.</para>
        /// <para>PropertyChanged raise on selected scheduler.</para>
        /// </summary>
        /// <param name="propertySelector">Argument is self, Return is target property.</param>
        /// <param name="mode">ReactiveProperty mode.</param>
        /// <param name="ignoreValidationErrorValue">Ignore validation error value.</param>
        public static ReadOnlyReactiveProperty<TProperty> ToReadOnlyReactivePropertyAsSynchronized<TSubject, TProperty>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            IScheduler raiseEventScheduler,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged
        {
            var result = subject.ObserveProperty(propertySelector, isPushCurrentValueAtFirst: true)
                .ToReadOnlyReactiveProperty(eventScheduler: raiseEventScheduler, mode: mode);

            return result;
        }

        /// <summary>
        /// <para>Converts NotificationObject's property to ReadOnlyReactiveProperty. Value is one-way synchronized.</para>
        /// <para>PropertyChanged raise on ReactivePropertyScheduler.</para>
        /// </summary>
        /// <param name="propertySelector">Argument is self, Return is target property.</param>
        /// <param name="convert">Convert selector to ReactiveProperty.</param>
        /// <param name="mode">ReactiveProperty mode.</param>
        /// <param name="ignoreValidationErrorValue">Ignore validation error value.</param>
        public static ReadOnlyReactiveProperty<TResult> ToReadOnlyReactivePropertyAsSynchronized<TSubject, TProperty, TResult>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            Func<TProperty, TResult> convert,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged =>
            ToReadOnlyReactivePropertyAsSynchronized(subject, propertySelector, convert, ReactivePropertyScheduler.Default, mode, ignoreValidationErrorValue);

        /// <summary>
        /// <para>Converts NotificationObject's property to ReadOnlyReactiveProperty. Value is one-way synchronized.</para>
        /// <para>PropertyChanged raise on selected scheduler.</para>
        /// </summary>
        /// <param name="propertySelector">Argument is self, Return is target property.</param>
        /// <param name="convert">Convert selector to ReactiveProperty.</param>
        /// <param name="mode">ReactiveProperty mode.</param>
        /// <param name="ignoreValidationErrorValue">Ignore validation error value.</param>
        public static ReadOnlyReactiveProperty<TResult> ToReadOnlyReactivePropertyAsSynchronized<TSubject, TProperty, TResult>(
            this TSubject subject,
            Expression<Func<TSubject, TProperty>> propertySelector,
            Func<TProperty, TResult> convert,
            IScheduler raiseEventScheduler,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged
        {
            var result = subject.ObserveProperty(propertySelector, isPushCurrentValueAtFirst: true)
                .Select(convert)
                .ToReadOnlyReactiveProperty(eventScheduler: raiseEventScheduler, mode: mode);

            return result;
        }
        
        public static IObservable<TProperty> ObserveProperty<TSubject, TProperty>(
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
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged =>
            ToReactivePropertyAsSynchronized(subject, propertyName, getter, setter, ReactivePropertyScheduler.Default, mode, ignoreValidationErrorValue);
        
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
            var result = subject.ObserveProperty(propertyName, getter, isPushCurrentValueAtFirst: true)
                .ToReactiveProperty(raiseEventScheduler, mode: mode);
            result
                .Where(_ => !ignoreValidationErrorValue || !result.HasErrors)
                .Subscribe(setter);

            return result;
        }
        
        public static ReactiveProperty<TResult> ToReactivePropertyAsSynchronized<TSubject, TProperty, TResult>(
            this TSubject subject,
            string propertyName,
            Func<TProperty> getter,
            Action<TProperty> setter,
            Func<TProperty, TResult> convert,
            Func<TResult, TProperty> convertBack,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged =>
            ToReactivePropertyAsSynchronized(subject, propertyName, getter, setter, convert, convertBack, ReactivePropertyScheduler.Default, mode, ignoreValidationErrorValue);
        
        public static ReactiveProperty<TResult> ToReactivePropertyAsSynchronized<TSubject, TProperty, TResult>(
            this TSubject subject,
            string propertyName,
            Func<TProperty> getter,
            Action<TProperty> setter,
            Func<TProperty, TResult> convert,
            Func<TResult, TProperty> convertBack,
            IScheduler raiseEventScheduler,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
            bool ignoreValidationErrorValue = false)
            where TSubject : INotifyPropertyChanged
        {
            var result = subject.ObserveProperty(propertyName, getter, isPushCurrentValueAtFirst: true)
                .Select(convert)
                .ToReactiveProperty(raiseEventScheduler, initialValue: convert(getter()), mode: mode);
            result
                .Where(_ => !ignoreValidationErrorValue || !result.HasErrors)
                .Select(convertBack)
                .Subscribe(setter);

            return result;
        }
    }
}
