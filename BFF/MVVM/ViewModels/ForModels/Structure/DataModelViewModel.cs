using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IDataModelViewModel {
        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        long Id { get; }

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
    public abstract class DataModelViewModel : ObservableObject, IDataModelViewModel
    {
        /// <summary>
        /// The Orm object, which handles all database accesses.
        /// </summary>
        protected IBffOrm Orm;

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public abstract long Id { get; }

        /// <summary>
        /// Initializes a DataModelViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        protected DataModelViewModel(IBffOrm orm)
        {
            Orm = orm;
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
        protected abstract void InsertToDb();

        /// <summary>
        /// Uses the OR mapper to update the object in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected abstract void UpdateToDb();

        /// <summary>
        /// Uses the OR mapper to delete the object from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected abstract void DeleteFromDb();

        /// <summary>
        /// Inserts the model object to the database.
        /// </summary>
        public void Insert()
        {
            if (Id == -1L && ValidToInsert()) InsertToDb();
        }

        /// <summary>
        /// Updates the model object in the database.
        /// </summary>
        protected void Update()
        {
            if (Id > 0L) UpdateToDb();
        }


        /// <summary>
        /// Deletes the model object from the database.
        /// </summary>
        public void Delete()
        {
            if (Id > 0L) DeleteFromDb();
        }
    }
}
