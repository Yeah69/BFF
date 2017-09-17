using System;

namespace BFF.DataVirtualizingObservableCollection
{
    public class RelayBasicDataAccess<T> : IBasicDataAccess<T>
    {
        private readonly Func<int, int, T[]> _pageFetcher;
        private readonly Func<int> _countFetcher;
        private readonly Func<T> _placeHolderFactory;

        public RelayBasicDataAccess(Func<int, int, T[]> pageFetcher, Func<int> countFetcher, Func<T> placeHolderFactory)
        {
            _pageFetcher = pageFetcher;
            _countFetcher = countFetcher;
            _placeHolderFactory = placeHolderFactory;
        }

        public T[] PageFetch(int offSet, int pageSize)
        {
            return _pageFetcher(offSet, pageSize);
        }

        public int CountFetch()
        {
            return _countFetcher();
        }

        public T CreatePlaceHolder()
        {
            return _placeHolderFactory();
        }
    }
}