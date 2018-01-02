using System;
using System.Reactive.Disposables;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel
    {

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        bool ValidToInsert();

        /// <summary>
        /// Inserts the model object to the database.
        /// </summary>
        void Insert();

        /// <summary>
        /// Deletes the model object from the database.
        /// </summary>
        void Delete();
    }

    /// <summary>
    /// Base class to all ViewModels for Models, which are read from the database.
    /// Offers CRUD functionality (except for Read, because the Models are not read from within itself).
    /// </summary>
    public abstract class DataModelViewModel : ViewModelBase, IDataModelViewModel, IDisposable, ITransientViewModel
    {
        private readonly IDataModel _dataModel;

        /// <summary>
        /// The Orm object, which handles all database accesses.
        /// </summary>
        protected IBffOrm Orm;

        /// <summary>
        /// Provides common properties, such as Accounts, Categories and Payees.
        /// </summary>
        protected ICommonPropertyProvider CommonPropertyProvider => Orm?.CommonPropertyProvider;

        protected CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();

        /// <summary>
        /// Initializes a DataModelViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="dataModel">The model.</param>
        protected DataModelViewModel(IBffOrm orm, IDataModel dataModel)
        {
            Orm = orm;
            _dataModel = dataModel;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public abstract bool ValidToInsert();

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
            if (ValidToInsert())
            {
                _dataModel.Insert();
                OnInsert();
            }
        }

        /// <summary>
        /// Updates the model object in the database.
        /// </summary>
        protected virtual void OnUpdate() {}


        /// <summary>
        /// Deletes the model object from the database.
        /// </summary>
        public void Delete() => _dataModel.Delete();

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) { }
            CompositeDisposable.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
