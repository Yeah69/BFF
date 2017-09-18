﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace BFF.DataVirtualizingObservableCollection
{
    public interface IDataVirtualizingCollectionBuilderRequired<T>
    {
        IDataVirtualizingCollectionBuilderOptional<T> With(IBasicDataAccess<T> dataAccess, IScheduler subscribeScheduler, IScheduler observeScheduler);
        IDataVirtualizingCollectionBuilderOptional<T> With(Func<int, int, T[]> pageFetcher, Func<int> countFetcher, Func<T> placeholderFactory, IScheduler subscribeScheduler, IScheduler observeScheduler);
    }
    public interface IDataVirtualizingCollectionBuilderOptional<T>
    {
        IDataVirtualizingCollectionBuilderOptional<T> WithPageSize(int pageSize);
        IDataVirtualizingCollection<T> Build();
    }
    
    public class DataVirtualizingCollection<T> : IDataVirtualizingCollection<T>
    {
        public static IDataVirtualizingCollectionBuilderRequired<T> CreateBuilder() => new Builder<T>();

        public class Builder<TItem> : IDataVirtualizingCollectionBuilderRequired<TItem>, IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            private IBasicDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private IScheduler _subscribeScheduler;
            private IScheduler _observeScheduler;

            public IDataVirtualizingCollectionBuilderOptional<TItem> With(IBasicDataAccess<TItem> dataAccess, IScheduler subscribeScheduler, IScheduler observeScheduler)
            {
                _dataAccess = dataAccess;
                _subscribeScheduler = subscribeScheduler;
                _observeScheduler = observeScheduler;
                return this;
            }

            public IDataVirtualizingCollectionBuilderOptional<TItem> With(Func<int, int, TItem[]> pageFetcher, Func<int> countFetcher, Func<TItem> placeholderFactory, IScheduler subscribeScheduler,
                IScheduler observeScheduler)
            {
                _dataAccess = new RelayBasicDataAccess<TItem>(pageFetcher, countFetcher, placeholderFactory);
                _subscribeScheduler = subscribeScheduler;
                _observeScheduler = observeScheduler;
                return this;
            }

            public IDataVirtualizingCollectionBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = 100;
                return this;
            }

            public IDataVirtualizingCollection<TItem> Build()
            {
                var dataVirtualizingCollection =
                    new DataVirtualizingCollection<TItem>(_dataAccess, _subscribeScheduler, _observeScheduler)
                    {
                        _pageSize = _pageSize
                    };
                return dataVirtualizingCollection;
            }
        }


        private readonly IBasicDataAccess<T> _dataAccess;

        private readonly int _count;
        private int _pageSize = 100;

        private readonly Subject<(int, T)> _itemRequests = new Subject<(int, T)>();
        private readonly Subject<int> _pageRequests = new Subject<int>();
        private readonly IDictionary<int, T[]> _pageStore = new Dictionary<int, T[]>();
        private readonly IDictionary<int, IList<(int, T)>> _itemRequestsStore = new Dictionary<int, IList<(int, T)>>();

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private DataVirtualizingCollection(IBasicDataAccess<T> dataAccess, IScheduler subscribeScheduler, IScheduler observeScheduler)
        {
            _dataAccess = dataAccess;
            _compositeDisposable.Add(_itemRequests);
            _compositeDisposable.Add(_pageRequests);

            _count = dataAccess.CountFetch();

            _pageRequests
                .Distinct()
                .Select(pageKey =>
                {
                    int offset = pageKey * _pageSize;
                    T[] page = _dataAccess.PageFetch(offset, _pageSize);
                    _pageStore[pageKey] = page;
                    return pageKey;
                })
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(observeScheduler)
                .Subscribe(pageKey =>
                {
                    T[] page = _pageStore[pageKey];
                    foreach (var tuple in _itemRequestsStore[pageKey])
                    {
                        OnCollectionChangedReplace(page[tuple.Item1], tuple.Item2, pageKey * _pageSize + tuple.Item1);
                    }
                });

            var itemRequestSubscription = _itemRequests
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(subscribeScheduler)
                .Subscribe(tuple =>
                {
                    int pageKey = tuple.Item1 / _pageSize;
                    int index = tuple.Item1 % _pageSize;
                    if (!_itemRequestsStore.ContainsKey(pageKey))
                    {
                        _itemRequestsStore[pageKey] = new List<(int, T)>();
                    }
                    _itemRequestsStore[pageKey].Add((index, tuple.Item2));
                });
            _compositeDisposable.Add(itemRequestSubscription);
        }

        private T GetItemInner(int index)
        {
            int pageIndex = index / _pageSize;
            if (_pageStore.ContainsKey(pageIndex))
                return _pageStore[pageIndex][index % _pageSize];

            var placeHolder = _dataAccess.CreatePlaceHolder();

            _pageRequests.OnNext(index / _pageSize);

            _itemRequests.OnNext((index, placeHolder));

            return placeHolder;
        }

        public T this[int index]
        {
            get => GetItemInner(index);
            set => throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get => GetItemInner(index);
            set => throw new NotSupportedException();
        }

        int GetCountInner() => _count;

        int ICollection<T>.Count => GetCountInner();

        int ICollection.Count => GetCountInner();

        public IEnumerator<T> GetEnumerator()
        {
            yield return _dataAccess.CreatePlaceHolder();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsFixedSize => true;

        bool IList.IsReadOnly => true;

        bool ICollection<T>.IsReadOnly => true;

        public bool IsSynchronized { get; } = false;
        public object SyncRoot { get; } = new object();

        public int IndexOf(object value)
        {
            return -1;
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            foreach (var disposable in _pageStore.SelectMany(ps => ps.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            _pageStore.Clear();
            foreach (var list in _itemRequestsStore.Values)
            {
                list.Clear();
            }
            _itemRequestsStore.Clear();
        }

        protected virtual void OnCollectionChangedReplace(T newItem, T oldItem, int index)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}