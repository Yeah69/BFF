using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Extensions;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using Reactive.Bindings.Extensions;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel : INotifyingErrorViewModel
    {
        Task InsertAsync();

        bool IsInsertable();
        
        Task DeleteAsync();

        ICommand? DeleteCommand { get; }

        bool IsInserted { get; }
    }

    public abstract class DataModelViewModel : NotifyingErrorViewModelBase, IDataModelViewModel, IDisposable
    {
        private readonly IDataModel _dataModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        protected CompositeDisposable CompositeDisposable { get; } = new();
        
        protected DataModelViewModel(
            IDataModel dataModel,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            _dataModel = dataModel;
            _rxSchedulerProvider = rxSchedulerProvider;

            dataModel
                .ObservePropertyChanged(nameof(IDataModel.IsInserted))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(IsInserted)))
                .AddTo(CompositeDisposable);

            DeleteCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(CompositeDisposable, DeleteAsync);
        }
        
        public virtual Task InsertAsync()
        {
            return _dataModel.InsertAsync();
        }

        public virtual bool IsInsertable() => _dataModel.IsInserted.Not();
        
        public virtual Task DeleteAsync()
        {
            return _dataModel.DeleteAsync();
        }

        public ICommand DeleteCommand { get; }
        public bool IsInserted => _dataModel.IsInserted;

        public void Dispose() =>
            _rxSchedulerProvider.UI.MinimalSchedule(() => CompositeDisposable.Dispose());
    }
}
