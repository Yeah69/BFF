using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel
    {
        Task InsertAsync();

        bool IsInsertable();
        
        Task DeleteAsync();

        ReactiveCommand DeleteCommand { get; }

        bool IsInserted { get; }
    }
    
    public abstract class DataModelViewModel : ViewModelBase, IDataModelViewModel, IDisposable
    {
        private readonly IDataModel _dataModel;
        private readonly IRxSchedulerProvider _schedulerProvider;

        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();
        
        protected DataModelViewModel(
            IDataModel dataModel,
            IRxSchedulerProvider schedulerProvider)
        {
            _dataModel = dataModel;
            _schedulerProvider = schedulerProvider;

            dataModel
                .ObservePropertyChanges(nameof(IDataModel.Id))
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(IsInserted)))
                .AddTo(CompositeDisposable);

            DeleteCommand = new ReactiveCommand().AddTo(CompositeDisposable);
            DeleteCommand.Subscribe(async _ => await DeleteAsync()).AddTo(CompositeDisposable);
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

        public ReactiveCommand DeleteCommand { get; }
        public bool IsInserted => _dataModel.Id > 0;

        public void Dispose() =>
            _schedulerProvider.UI.MinimalSchedule(() => CompositeDisposable.Dispose());
    }
}
