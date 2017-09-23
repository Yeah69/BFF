using System.Reactive.Concurrency;

namespace BFF.DataVirtualizingObservableCollection
{
    /// <summary>
    /// Is intended to provide a convenient way to build data virtualizing collections for consumers.
    /// Each function has the required components as normal parameters and the optionally configurable components as optional parameters.
    /// The names of the functions describe which kind of collection is build.
    /// </summary>
    /// <typeparam name="T">The type of the elements provided by the build collection.</typeparam>
    public interface ICollectionBuilder<T>
    {
        /// <summary>
        /// Builds a data virtualizing collection which operates async (i.e. placeholders are returned if element is not available yet)
        /// and hoards the fetched pages (i.e. the once fetched pages of data are kept in memory for the life time of the collection).
        /// </summary>
        /// <param name="dataAccess">Provides access to the data, see <see cref="IBasicDataAccess{T}"/></param>
        /// <param name="subscribeScheduler">Is used to schedule task, which should be done in the background. 
        /// Basically, it is everything besides the notifications, like fetching the pages.
        /// It is advised to used the ThreadPoolScheduler in order to do the heavy lifting in the background.</param>
        /// <param name="observeScheduler">Is used to schedule the notification. For GUI applications it is mandatory to use the DispatcherScheduler. </param>
        /// <param name="pageSize">Optionally, the page size can be configured, whereas a page size of 100 elements is considered as universal enough to be the default.
        /// A lesser page size will consequently mean more frequent page fetches and a greater page size mean a bigger chunk of data is fetched at once.
        /// This parameter allows adjusting to specific requirements.</param>
        /// <returns></returns>
        IDataVirtualizingCollection<T> BuildAHoardingAsyncCollection(
            IBasicDataAccess<T> dataAccess,
            IScheduler subscribeScheduler,
            IScheduler observeScheduler,
            int pageSize = 100);

        IDataVirtualizingCollection<T> BuildAHoardingSyncCollection(
            IBasicDataAccess<T> dataAccess,
            IScheduler subscribeScheduler,
            IScheduler observeScheduler,
            int pageSize = 100);
    }

    /// <inheritdoc />
    public class CollectionBuilder<T> : ICollectionBuilder<T>
    {
        public static ICollectionBuilder<T> CreateBuilder() => new CollectionBuilder<T>();

        public IDataVirtualizingCollection<T> BuildAHoardingAsyncCollection(
            IBasicDataAccess<T> dataAccess, 
            IScheduler subscribeScheduler,
            IScheduler observeScheduler,
            int pageSize = 100)
        {
            var hoardingPageStore = HoardingPageStore<T>
                .CreateBuilder()
                .With(
                    dataAccess, 
                    subscribeScheduler)
                .WithPageSize(pageSize)
                .Build();
            return DataVirtualizingCollection<T>
                .CreateBuilder()
                .WithPageStore(
                    hoardingPageStore, 
                    dataAccess, 
                    subscribeScheduler, 
                    observeScheduler)
                .Build();
        }

        public IDataVirtualizingCollection<T> BuildAHoardingSyncCollection(IBasicDataAccess<T> dataAccess, IScheduler subscribeScheduler,
            IScheduler observeScheduler, int pageSize = 100)
        {
            var hoardingPageStore = HoardingSyncPageStore<T>
                .CreateBuilder()
                .With(dataAccess)
                .WithPageSize(pageSize)
                .Build();
            return DataVirtualizingCollection<T>
                .CreateBuilder()
                .WithPageStore(
                    hoardingPageStore,
                    dataAccess,
                    subscribeScheduler,
                    observeScheduler)
                .Build();
        }

        private CollectionBuilder() 
        {
        }
    }
}