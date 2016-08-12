namespace BFF.MVVM.ViewModels.ForModels
{
    public abstract class DbViewModelBase : ObservableObject
    {
        public abstract long Id { get; }

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
