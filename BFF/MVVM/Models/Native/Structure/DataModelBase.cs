using BFF.DB;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Base class for all model classes, which get OR-mapped
    /// </summary>
    public abstract class DataModelBase : ObservableObject
    {
        protected bool ConstrDbLock;

        /// <summary>
        /// Identification number for the database
        /// </summary>
        [Key]
        public long Id { get; set; } = -1L;

        /// <summary>
        /// Reference to current ORM class
        /// </summary>
        [Write(false)]
        public static IBffOrm Database { get; set; }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        protected DataModelBase(long id = -1L)
        {
            ConstrDbLock = true;

            if (Id == -1L || id > 0L) Id = id;

            ConstrDbLock = false;
        }

        protected abstract void InsertToDb();
        public abstract bool ValidToInsert();

        /// <summary>
        /// Inserts this object to the database
        /// </summary>
        public void Insert()
        {
            if(Id == -1L && ValidToInsert()) InsertToDb();
        }

        protected abstract void UpdateToDb();

        /// <summary>
        /// Updates this object in the database
        /// </summary>
        protected void Update()
        {
            if(!ConstrDbLock) UpdateToDb();
        }

        protected abstract void DeleteFromDb();

        /// <summary>
        /// Deletes this object from the database
        /// </summary>
        public void Delete()
        {
            if(Id > 0L) DeleteFromDb();
        }
    }
}
