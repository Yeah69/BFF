using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.Helper
{
    public static class ReactivePropertyHelper
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
    }
}
