using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public abstract class DataModelViewModel : ObservableObject
    {
        protected IBffOrm Orm;
        public abstract long Id { get; }

        protected DataModelViewModel(IBffOrm orm)
        {
            Orm = orm;
        }

        internal abstract bool ValidToInsert();
        protected abstract void InsertToDb();
        protected abstract void UpdateToDb();
        protected abstract void DeleteFromDb();

        /// <summary>
        /// Inserts the model object to the database
        /// </summary>
        public void Insert()
        {
            if (Id == -1L && ValidToInsert()) InsertToDb();
        }

        /// <summary>
        /// Updates the model object in the database
        /// </summary>
        protected void Update()
        {
            UpdateToDb();
        }


        /// <summary>
        /// Deletes the model object from the database
        /// </summary>
        public void Delete()
        {
            if (Id > 0L) DeleteFromDb();
        }
    }
}
