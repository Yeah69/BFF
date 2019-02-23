using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models.Structure
{
    public interface IDataModel : IObservableObject
    {
        bool IsInserted { get; }

        Task InsertAsync();

        Task DeleteAsync();
    }
    internal interface IDataModelInternal<T> : IDataModel
        where T : class, IPersistenceModel
    {
        T BackingPersistenceModel { get; }
    }

    internal abstract class DataModel<TDomain, TPersistence> : ObservableObject, IDataModelInternal<TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
        private readonly IWriteOnlyRepository<TDomain> _repository;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly bool _isInserted;

        protected DataModel(
            TPersistence backingPersistenceModel, 
            bool isInserted, 
            IWriteOnlyRepository<TDomain> repository, 
            IRxSchedulerProvider rxSchedulerProvider)
        {
            _repository = repository;
            _rxSchedulerProvider = rxSchedulerProvider;
            _isInserted = isInserted;
            BackingPersistenceModel = backingPersistenceModel;
        }

        public TPersistence BackingPersistenceModel { get; }

        public bool IsInserted => _isInserted;

        public virtual async Task InsertAsync()
        {
            await BackingPersistenceModel.InsertAsync().ConfigureAwait(false);
        }

        protected virtual async Task UpdateAsync()
        {
            await BackingPersistenceModel.UpdateAsync().ConfigureAwait(false);
        }
        
        public virtual async Task DeleteAsync()
        {
            await BackingPersistenceModel.DeleteAsync().ConfigureAwait(false);
        }

        protected Task UpdateAndNotify([CallerMemberName] string propertyName = "") => 
            Task.Run(UpdateAsync)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Do(_ => OnPropertyChanged(propertyName))
                .ToTask();
    }
}
