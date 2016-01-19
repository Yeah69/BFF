using BFF.DB;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    /// <summary>
    /// Base class for all model classes, which get OR-mapped
    /// </summary>
    public abstract class DataModelBase : ObservableObject
    {
        protected bool ConstrDbLock = false;

        /// <summary>
        /// Identification number for the database
        /// </summary>
        [Key]
        public long Id { get; set; } = -1;

        /// <summary>
        /// Reference to current ORM class
        /// </summary>
        [Write(false)]
        public static IBffOrm Database { get; set; }

        protected abstract void InsertToDb();

        public void Insert()
        {
            if(Id == -1L) InsertToDb();
        }

        protected abstract void UpdateToDb();

        protected void Update()
        {
            if(!ConstrDbLock) UpdateToDb();
        }

        protected abstract void DeleteFromDb();

        public void Delete()
        {
            if(Id > 0L) DeleteFromDb();
        }
    }
}
