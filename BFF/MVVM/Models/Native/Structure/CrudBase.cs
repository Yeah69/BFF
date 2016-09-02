namespace BFF.MVVM.Models.Native.Structure
{
    public interface ICrudBase : IDataModelBase {
        bool ValidToInsert();

        /// <summary>
        /// Inserts this object to the database
        /// </summary>
        void Insert();

        /// <summary>
        /// Deletes this object from the database
        /// </summary>
        void Delete();
    }

    public abstract class CrudBase : DataModelBase, ICrudBase
    {

        protected abstract void InsertToDb();
        public abstract bool ValidToInsert();

        /// <summary>
        /// Inserts this object to the database
        /// </summary>
        public void Insert()
        {
            if (Id == -1L && ValidToInsert()) InsertToDb();
        }

        protected abstract void UpdateToDb();

        /// <summary>
        /// Updates this object in the database
        /// </summary>
        protected void Update()
        {
            if (!ConstrDbLock) UpdateToDb();
        }

        protected abstract void DeleteFromDb();

        /// <summary>
        /// Deletes this object from the database
        /// </summary>
        public void Delete()
        {
            if (Id > 0L) DeleteFromDb();
        }
    }
}
