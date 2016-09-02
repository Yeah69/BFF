﻿using BFF.DB;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface IDataModelBase
    {
        /// <summary>
        /// Identification number for the database
        /// </summary>
        long Id { get; set; }
    }

    /// <summary>
    /// Base class for all model classes, which get OR-mapped
    /// </summary>
    public abstract class DataModelBase : ObservableObject, IDataModelBase
    {
        protected bool ConstrDbLock;

        private long _id = -1L;

        /// <summary>
        /// Identification number for the database
        /// </summary>
        [Key]
        public long Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

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
    }
}
