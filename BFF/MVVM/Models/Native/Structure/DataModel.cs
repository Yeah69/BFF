using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BFF.DB;
using BFF.Helper;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface IDataModel : IObservableObject
    {
        long Id { get; set; }
        
        Task InsertAsync();
        
        Task UpdateAsync();
        
        Task DeleteAsync();
    }

    public abstract class DataModel<T> : ObservableObject, IDataModel where T : class, IDataModel
    {
        private readonly IWriteOnlyRepository<T> _repository;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public long Id { get; set; } = -1L;
        
        protected DataModel(IWriteOnlyRepository<T> repository, IRxSchedulerProvider rxSchedulerProvider, long id)
        {
            _repository = repository;
            _rxSchedulerProvider = rxSchedulerProvider;
            if (Id == -1L || id > 0L) Id = id;
        }
        
        public virtual async Task InsertAsync()
        {
            await _repository.AddAsync(this as T).ConfigureAwait(false);
        }
        
        public virtual async Task UpdateAsync()
        {
            await _repository.UpdateAsync(this as T).ConfigureAwait(false);
        }
        
        public virtual async Task DeleteAsync()
        {
            await _repository.DeleteAsync(this as T).ConfigureAwait(false);
        }

        protected Task UpdateAndNotify([CallerMemberName] string propertyName = "") => 
            Task.Run(UpdateAsync)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Do(_ => OnPropertyChanged(propertyName))
                .ToTask();
    }
}
