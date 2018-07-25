using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel : INotifyPropertyChanged
    {
        Task InsertAsync();

        bool IsInsertable();
        
        Task DeleteAsync();

        IRxRelayCommand DeleteCommand { get; }

        bool IsInserted { get; }
    }
    
    public abstract class DataModelViewModel : ViewModelBase, IDataModelViewModel, IDisposable
    {
        private readonly IDataModel _dataModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();
        
        protected DataModelViewModel(
            IDataModel dataModel,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            _dataModel = dataModel;
            _rxSchedulerProvider = rxSchedulerProvider;

            dataModel
                .ObservePropertyChanges(nameof(IDataModel.Id))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(IsInserted)))
                .AddTo(CompositeDisposable);

            DeleteCommand = new AsyncRxRelayCommand(async () => await DeleteAsync()).AddTo(CompositeDisposable);
        }
        
        public virtual async Task InsertAsync()
        {
            await _dataModel.InsertAsync();
        }

        public virtual bool IsInsertable() => _dataModel.Id <= 0;
        
        protected virtual void OnUpdate() {}
        
        public virtual async Task DeleteAsync()
        {
            await _dataModel.DeleteAsync();
        }

        public IRxRelayCommand DeleteCommand { get; }
        public bool IsInserted => _dataModel.Id > 0;

        public void Dispose() =>
            _rxSchedulerProvider.UI.MinimalSchedule(() => CompositeDisposable.Dispose());
    }
}
