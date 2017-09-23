using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace BFF.DataVirtualizingObservableCollection
{
    /// <summary>
    /// Operates in sync way, which means that it does block the current thread if the element isn't available yet.
    /// Additionally, it keeps all already fetched pages in memory until it is garbage collected.
    /// On Dispose all stored disposable elements are disposed before this store disposes itself.
    /// </summary>
    /// <typeparam name="T">The type of the stored elements.</typeparam>
    public interface IHoardingSyncPageStore<T> : IPageStore<T>
    {
    }

    /// <inheritdoc />
    internal class HoardingSyncPageStore<T> : IHoardingSyncPageStore<T>
    {
        internal static IHoardingSyncPageStoreBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IHoardingSyncPageStoreBuilderOptional<TItem>
        {
            IHoardingSyncPageStoreBuilderOptional<TItem> WithPageSize(int pageSize);

            IHoardingSyncPageStore<TItem> Build();
        }

        internal interface IHoardingSyncPageStoreBuilderRequired<TItem>
        {
            IHoardingSyncPageStoreBuilderOptional<TItem> With(IBasicDataAccess<TItem> dataAccess);
        }

        internal class Builder<TItem> : IHoardingSyncPageStoreBuilderRequired<TItem>, IHoardingSyncPageStoreBuilderOptional<TItem>
        {
            private IBasicDataAccess<TItem> _dataAccess;
            private int _pageSize = 100;

            public IHoardingSyncPageStoreBuilderOptional<TItem> With(IBasicDataAccess<TItem> dataAccess)
            {
                _dataAccess = dataAccess;
                return this;
            }

            public IHoardingSyncPageStoreBuilderOptional<TItem> WithPageSize(int pageSize)
            {
                _pageSize = pageSize;
                return this;
            }

            public IHoardingSyncPageStore<TItem> Build()
            {
                return new HoardingSyncPageStore<TItem>(_dataAccess)
                {
                    _pageSize = _pageSize
                };
            }
        }

        private readonly IPageFetcher<T> _pageFetcher;
        private int _pageSize = 100;
        
        private readonly IDictionary<int, T[]> _pageStore = new Dictionary<int, T[]>();

        private HoardingSyncPageStore(IPageFetcher<T> pageFetcher)
        {
            OnCollectionChangedReplace = Observable.Never<(T, T, int)>();
            _pageFetcher = pageFetcher;
        }

        public T Fetch(int index)
        {
            int pageKey = index / _pageSize;
            int pageIndex = index % _pageSize;

            if (!_pageStore.ContainsKey(pageKey))
            {
                _pageStore[pageKey] = _pageFetcher.PageFetch(pageKey * _pageSize, _pageSize);
            }

            return _pageStore[pageKey][pageIndex];
        }

        public IObservable<(T, T, int)> OnCollectionChangedReplace { get; }

        public void Dispose()
        {
            foreach (var disposable in Enumerable.SelectMany(_pageStore, ps => ps.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}