using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface IDataModel : IObservableObject
    {
        /// <summary>
        /// Identification number for the database
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// The model inserts itself to a database provided by given ORM.
        /// </summary>
        void Insert();

        /// <summary>
        /// The model updates itself in a database provided by given ORM.
        /// </summary>
        void Update();

        /// <summary>
        /// The model removes itself from a database provided by given ORM.
        /// </summary>
        void Delete();
    }

    /// <summary>
    /// Base class for all model classes, which get OR-mapped
    /// </summary>
    public abstract class DataModel<T> : ObservableObject, IDataModel where T : class, IDataModel
    {
        private readonly IWriteOnlyRepository<T> _repository;

        /// <summary>
        /// Identification number for the database
        /// </summary>
        public long Id { get; set; } = -1L;
        
        protected DataModel(IWriteOnlyRepository<T> repository, long id)
        {
            _repository = repository;
            if (Id == -1L || id > 0L) Id = id;
        }

        /// <summary>
        /// The model inserts itself to a database provided by given ORM.
        /// </summary>
        public virtual void Insert()
        {
            _repository.Add(this as T).Wait();
        }

        /// <summary>
        /// The model updates itself in a database provided by given ORM.
        /// </summary>
        public virtual void Update()
        {
            _repository.Update(this as T).Wait();
        }

        /// <summary>
        /// The model removes itself from a database provided by given ORM.
        /// </summary>
        public virtual void Delete()
        {
            _repository.Delete(this as T).Wait();
        }
    }
}
