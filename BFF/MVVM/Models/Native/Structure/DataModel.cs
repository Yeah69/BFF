using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BFF.DB;

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
        
        public long Id { get; set; } = -1L;
        
        protected DataModel(IWriteOnlyRepository<T> repository, long id)
        {
            _repository = repository;
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
            Task.Run(UpdateAsync).ContinueWith(_ => OnPropertyChanged(propertyName));
    }
}
