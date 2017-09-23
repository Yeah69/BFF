using System;

namespace BFF.DataVirtualizingObservableCollection
{
    /// <summary>
    /// An implementation of the <see cref="IBasicDataAccess{T}"/>, which gets the access functions injected.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public class RelayBasicDataAccess<T> : IBasicDataAccess<T>
    {
        private readonly Func<int, int, T[]> _pageFetcher;
        private readonly Func<int> _countFetcher;
        private readonly Func<T> _placeHolderFactory;

        /// <summary>
        /// Initializes the <see cref="RelayBasicDataAccess{T}"/> object by injecting the access functions.
        /// </summary>
        /// <param name="pageFetcher">A function to fetch a page.</param>
        /// <param name="countFetcher">A function to fetch the item count.</param>
        /// <param name="placeHolderFactory">A function to create a placeholder.</param>
        public RelayBasicDataAccess(Func<int, int, T[]> pageFetcher, Func<int> countFetcher, Func<T> placeHolderFactory)
        {
            _pageFetcher = pageFetcher;
            _countFetcher = countFetcher;
            _placeHolderFactory = placeHolderFactory;
        }

        /// <inheritdoc />
        public T[] PageFetch(int offSet, int pageSize)
        {
            return _pageFetcher(offSet, pageSize);
        }

        /// <inheritdoc />
        public int CountFetch()
        {
            return _countFetcher();
        }

        /// <inheritdoc />
        public T CreatePlaceholder()
        {
            return _placeHolderFactory();
        }
    }
}