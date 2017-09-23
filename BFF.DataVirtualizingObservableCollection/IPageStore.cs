using System;

namespace BFF.DataVirtualizingObservableCollection
{
    /// <summary>
    /// Stores pages retrieved from some data access.
    /// The disposable part provides a way to dispose of all element which are stored in the page store.
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
}