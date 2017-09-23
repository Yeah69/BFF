using System;
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
        IDataVirtualizingCollectionBuilderOptional<T> WithHoardingPageStore(
            IHoardingPageStore<T> pageStore, 
            ICountFetcher countFetcher,
            IScheduler observeScheduler);
    }
    public interface IDataVirtualizingCollectionBuilderOptional<T>
    {
        IDataVirtualizingCollection<T> Build();
    }

    /// <summary>
    /// Stores pages retrieved from some data access.
    /// </summary>
    /// <typeparam name="T">Type of the elements stored in the data access.</typeparam>
    public interface IPageStore<T> : IDisposable
    {
        /// <summary>
        /// Tries to fetch
        /// </summary>
        /// <returns>IsSuccess is true if the Element could be retrieved; otherwise, false.
        /// Element is the requested element from data access, if the fetch was successful.</returns>
        T Fetch(int index);

        /// <summary>
        /// Sequence on pending replacements in the collection.
        /// First argument is new element,
        /// Second argument is the placeholder,
        /// Third argument is the index of the element in the data access.
        /// </summary>
        IObservable<(T, T, int)> OnCollectionChangedReplace { get; }
    }

    public interface IHoardingPageStoreBuilderRequired<T>
    {
        IHoardingPageStoreBuilderOptional<T> With(IBasicDataAccess<T> dataAccess, IScheduler subscribeScheduler);
    }
    public interface IHoardingPageStoreBuilderOptional<T>
    {
        IHoardingPageStoreBuilderOptional<T> WithPageSize(int pageSize);

        IHoardingPageStore<T> Build();
    }

    public interface IHoardingPageStore<T> : IPageStore<T>
    {
        IScheduler SubscribeScheduler { get; }
    }

    public class HoardingPageStore<T> : IHoardingPageStore<T>
    {
        public static IHoardingPageStoreBuilderRequired<T> CreateBuilder() => new Builder<T>(); 

        public class Builder<TItem> : IHoardingPageStoreBuilderRequired<TItem>, IHoardingPageStoreBuilderOptional<TItem>
        {
            private IBasicDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;
            private IScheduler _subscribeScheduler;

            public IHoardingPageStoreBuilderOptional<TItem> With(IBasicDataAccess<TItem> dataAccess, IScheduler subscribeScheduler)
            {
                _dataAccess = dataAccess;
                _subscribeScheduler = subscribeScheduler;
                return this;
            }

            public IHoardingPageStoreBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingPageStore<TItem> Build()
            {
                return new HoardingPageStore<TItem>(_dataAccess, _dataAccess, _subscribeScheduler)
                {
                    _pageSize = _pageSize
                };
            }
        }

        private readonly IPlaceholderFactory<T> _placeholderFactory;
        private int _pageSize = 100;

        private readonly Subject<int> _pageRequests = new Subject<int>();

        private readonly IDictionary<int, ReplaySubject<(int, T)>> _deferredRequests = new Dictionary<int, ReplaySubject<(int, T)>>();
        private readonly IDictionary<int, T[]> _pageStore = new Dictionary<int, T[]>();

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public HoardingPageStore(IPageFetcher<T> pageFetcher, IPlaceholderFactory<T> placeholderFactory, IScheduler subscribeScheduler)
        {
            _placeholderFactory = placeholderFactory;
            SubscribeScheduler = subscribeScheduler;

            var onCollectionChangedReplace = new Subject<(T, T, int)>();
            OnCollectionChangedReplace = onCollectionChangedReplace;

            _compositeDisposable.Add(onCollectionChangedReplace);
            _compositeDisposable.Add(_pageRequests);

            _pageRequests
                .Distinct()
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(subscribeScheduler)
                .Subscribe(pageKey =>
                {
                    int offset = pageKey * _pageSize;
                    T[] page = pageFetcher.PageFetch(offset, _pageSize);
                    _pageStore[pageKey] = page;
                    if (_deferredRequests.ContainsKey(pageKey))
                    {
                        var disposable = _deferredRequests[pageKey]
                            .Distinct()
                            .SubscribeOn(subscribeScheduler)
                            .ObserveOn(subscribeScheduler)
                            .Subscribe(tuple =>
                            {
                                onCollectionChangedReplace.OnNext(
                                    (page[tuple.Item1], tuple.Item2, pageKey * _pageSize + tuple.Item1));
                            }, () => _deferredRequests.Remove(pageKey));
                        _compositeDisposable.Add(disposable);
                    }
                });
        }

        public T Fetch(int index)
        {
            int pageKey = index / _pageSize;
            int pageIndex = index % _pageSize;

            if (_pageStore.ContainsKey(pageKey))
            {
                if (_deferredRequests.ContainsKey(pageKey))
                    _deferredRequests[pageKey].OnCompleted();

                return _pageStore[pageKey][pageIndex];
            }

            var placeholder = _placeholderFactory.CreatePlaceholder();

            if (!_deferredRequests.ContainsKey(pageKey))
                _deferredRequests[pageKey] = new ReplaySubject<(int, T)>();
            _deferredRequests[pageKey].OnNext((pageIndex, placeholder));

            _pageRequests.OnNext(pageKey);

            return placeholder;
        }

        public IObservable<(T, T, int)> OnCollectionChangedReplace { get; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            foreach (var disposable in _pageStore.SelectMany(ps => ps.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            foreach (var subject in _deferredRequests.Values)
            {
                subject.Dispose();
            }
        }

        public IScheduler SubscribeScheduler { get; }
    }

    public class DataVirtualizingCollection<T> : IDataVirtualizingCollection<T>
    {
        public static IDataVirtualizingCollectionBuilderRequired<T> CreateBuilder() => new Builder<T>();

        public class Builder<TItem> : IDataVirtualizingCollectionBuilderRequired<TItem>, IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            private IPageStore<TItem> _pageStore;
            private IScheduler _subscribeScheduler;
            private IScheduler _observeScheduler;
            private ICountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new DataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher, 
                    _subscribeScheduler,
                    _observeScheduler);
            }

            public IDataVirtualizingCollectionBuilderOptional<TItem> WithHoardingPageStore(
                IHoardingPageStore<TItem> pageStore,
                ICountFetcher countFetcher,
                IScheduler observeScheduler)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                _observeScheduler = observeScheduler;
                _subscribeScheduler = pageStore.SubscribeScheduler;
                return this;
            }
        }

        private readonly int _count;

        private IPageStore<T> _pageStore;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private DataVirtualizingCollection(IPageStore<T> pageStore, ICountFetcher countFetcher, IScheduler subscribeScheduler, IScheduler observeScheduler)
        {
            _pageStore = pageStore;

            var disposable = _pageStore.OnCollectionChangedReplace
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(observeScheduler)
                .Subscribe(tuple => OnCollectionChangedReplace(tuple.Item1, tuple.Item2, tuple.Item3));
            _compositeDisposable.Add(disposable);
            _compositeDisposable.Add(_pageStore);

            _count = countFetcher.CountFetch();
        }

        private T GetItemInner(int index)
        {
            return _pageStore.Fetch(index);
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
            return Enumerable.Empty<T>().GetEnumerator();
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