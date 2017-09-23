namespace BFF.DataVirtualizingObservableCollection
{
    /// <summary>
    /// The fundamental interface of the data virtualizing collection to get access to the data.
    /// It should be possible to fetch the count and pages of data of arbitrary size.
    /// Additionally, a factory method is required as well.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public interface IBasicDataAccess<out T> : IPageFetcher<T>, ICountFetcher, IPlaceholderFactory<T>
    {
    }

    /// <summary>
    /// Provides a function to fetch a page from a data access.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public interface IPageFetcher<out T>
    {
        /// <summary>
        /// Fetches a page of <see cref="pageSize"/> starting at the <see cref="offSet"/>.
        /// </summary>
        /// <param name="offSet">Where the fetched page should start.</param>
        /// <param name="pageSize">Number of item of the fetched page.</param>
        /// <returns>See summary.</returns>
        T[] PageFetch(int offSet, int pageSize);
    }

    /// <summary>
    /// Provides a function to fetch the count of elements stored in the data access.
    /// </summary>
    public interface ICountFetcher
    {
        /// <summary>
        /// Fetches the current count of all items, which can be accessed through this collection.
        /// </summary>
        /// <returns>See summary.</returns>
        int CountFetch();
    }

    /// <summary>
    /// Provides a function to create placeholder elements.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public interface IPlaceholderFactory<out T>
    {
        /// <summary>
        /// Creates a representative placeholder object of type T.
        /// </summary>
        /// <returns>See summary.</returns>
        T CreatePlaceholder();
    }
}