﻿using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Repositories;

namespace BFF.Model.Models.Structure
{
    public interface IDataModel : IObservableObject
    {
        long Id { get; set; }

        bool IsInserted { get; }
        
        Task InsertAsync();
        
        Task DeleteAsync();
    }

    internal abstract class DataModel<T> : ObservableObject, IDataModel where T : class, IDataModel
    {
        private readonly IWriteOnlyRepository<T> _repository;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private long _id = -1L;

        public long Id
        {
            get => _id;
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInserted));
            }
        }

        protected DataModel(IWriteOnlyRepository<T> repository, IRxSchedulerProvider rxSchedulerProvider, long id)
        {
            _repository = repository;
            _rxSchedulerProvider = rxSchedulerProvider;
            if (Id == -1L || id > 0L) Id = id;
        }

        public bool IsInserted => Id > 0L;

        public virtual async Task InsertAsync()
        {
            await _repository.AddAsync(this as T).ConfigureAwait(false);
        }

        protected virtual async Task UpdateAsync()
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
