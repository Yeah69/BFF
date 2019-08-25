using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using MrMeeseeks.Extensions;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel : INotifyingErrorViewModel, INotifyPropertyChanged
    {
        Task InsertAsync();

        bool IsInsertable();
        
        Task DeleteAsync();

        IRxRelayCommand DeleteCommand { get; }

        bool IsInserted { get; }
    }

    internal abstract class DataModelViewModel : NotifyingErrorViewModelBase, IDataModelViewModel, IDisposable
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
                .ObservePropertyChanges(nameof(IDataModel.IsInserted))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(IsInserted)))
                .AddTo(CompositeDisposable);

            DeleteCommand = new AsyncRxRelayCommand(DeleteAsync).AddTo(CompositeDisposable);
        }
        
        public virtual Task InsertAsync()
        {
            return _dataModel.InsertAsync();
        }

        public virtual bool IsInsertable() => _dataModel.IsInserted.Not();
        
        protected virtual void OnUpdate() {}
        
        public virtual Task DeleteAsync()
        {
            return _dataModel.DeleteAsync();
        }

        public IRxRelayCommand DeleteCommand { get; }
        public bool IsInserted => _dataModel.IsInserted;

        public void Dispose() =>
            _rxSchedulerProvider.UI.MinimalSchedule(() => CompositeDisposable.Dispose());
    }
}
