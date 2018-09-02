﻿using System;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;

namespace BFF.Helper.Extensions
{
    public static class NotifyCollectionChangedExtensions
    {
        public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>> ObserveCollectionChanges(
            this INotifyCollectionChanged source)
        {
            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => handler.Invoke,
                h => source.CollectionChanged += h,
                h => source.CollectionChanged -= h);
        }
    }
}
