using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel
    {
        void Insert();

        bool IsInsertable();
        
        void Delete();

        ReactiveCommand DeleteCommand { get; }

        IReadOnlyReactiveProperty<bool> IsInserted { get; }
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

            IsInserted = dataModel
                .ObserveProperty(d => d.Id)
                .Select(id => id > 0L)
                .ToReadOnlyReactivePropertySlim(dataModel.Id > 0L, ReactivePropertyMode.DistinctUntilChanged);

            DeleteCommand = new ReactiveCommand().AddTo(CompositeDisposable);
            DeleteCommand.Subscribe(_ => Delete()).AddTo(CompositeDisposable);
        }
        
        protected virtual void OnInsert() { }
        
        public void Insert()
        {
            _dataModel.InsertAsync();
            OnInsert();
        }

        public virtual bool IsInsertable() => _dataModel.Id <= 0;
        
        protected virtual void OnUpdate() {}
        
        public virtual void Delete()
        {
            _dataModel.DeleteAsync();
        }

        public ReactiveCommand DeleteCommand { get; }
        public IReadOnlyReactiveProperty<bool> IsInserted { get; }

        public void Dispose() =>
            _schedulerProvider.UI.MinimalSchedule(() => CompositeDisposable.Dispose());
    }
}
