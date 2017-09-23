using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BFF.DataVirtualizingObservableCollection
{
    /// <summary>
    /// Operates in async way, which means that it doesn't block the current thread and in case the element isn't available a placeholder is provided.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    public interface IHoardingPageStore<T> : IPageStore<T>
    {
    }

    /// <inheritdoc />
    internal class HoardingPageStore<T> : IHoardingPageStore<T>
    {
        internal static IHoardingPageStoreBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IHoardingPageStoreBuilderOptional<TItem>
        {
            IHoardingPageStoreBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingPageStore<TItem> Build();
        }

        internal interface IHoardingPageStoreBuilderRequired<TItem>
        {
            IHoardingPageStoreBuilderOptional<TItem> With(IBasicDataAccess<TItem> dataAccess, IScheduler subscribeScheduler);
        }

        internal class Builder<TItem> : IHoardingPageStoreBuilderRequired<TItem>, IHoardingPageStoreBuilderOptional<TItem>
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

        private HoardingPageStore(IPageFetcher<T> pageFetcher, IPlaceholderFactory<T> placeholderFactory, IScheduler subscribeScheduler)
        {
            _placeholderFactory = placeholderFactory;

            var onCollectionChangedReplace = new Subject<(T, T, int)>();
            OnCollectionChangedReplace = onCollectionChangedReplace;

            _compositeDisposable.Add(onCollectionChangedReplace);
            _compositeDisposable.Add(_pageRequests);

            _pageRequests.Distinct()
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(subscribeScheduler)
                .Subscribe(pageKey =>
                {
                    int offset = pageKey * _pageSize;
                    T[] page = pageFetcher.PageFetch(offset, _pageSize);
                    _pageStore[pageKey] = page;
                    if (_deferredRequests.ContainsKey(pageKey))
                    {
                        var disposable = _deferredRequests[pageKey].Distinct()
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
    }
}