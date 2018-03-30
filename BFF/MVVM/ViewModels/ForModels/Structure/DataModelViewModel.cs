using System;
using System.Reactive.Disposables;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel
    {
        /// <summary>
        /// Inserts the model object to the database.
        /// </summary>
        void Insert();

        bool IsInsertable();

        /// <summary>
        /// Deletes the model object from the database.
        /// </summary>
        void Delete();

        ReactiveCommand DeleteCommand { get; }
    }

    /// <summary>
    /// Base class to all ViewModels for Models, which are read from the database.
    /// Offers CRUD functionality (except for Read, because the Models are not read from within itself).
    /// </summary>
    public abstract class DataModelViewModel : ViewModelBase, IDataModelViewModel, IDisposable
    {
        private readonly IDataModel _dataModel;

        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();

        /// <summary>
        /// Initializes a DataModelViewModel.
        /// </summary>
        /// <param name="dataModel">The model.</param>
        protected DataModelViewModel(IDataModel dataModel)
        {
            _dataModel = dataModel;

            DeleteCommand = new ReactiveCommand().AddTo(CompositeDisposable);
            DeleteCommand.Subscribe(_ => Delete()).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Uses the OR mapper to insert the object into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected virtual void OnInsert() { }


        /// <summary>
        /// Inserts the model object to the database.
        /// </summary>
        public void Insert()
        {
            _dataModel.InsertAsync();
            OnInsert();
        }

        public virtual bool IsInsertable() => _dataModel.Id <= 0;

        /// <summary>
        /// Updates the model object in the database.
        /// </summary>
        protected virtual void OnUpdate() {}


        /// <summary>
        /// Deletes the model object from the database.
        /// </summary>
        public virtual void Delete()
        {
            _dataModel.DeleteAsync();
        }

        public ReactiveCommand DeleteCommand { get; }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
