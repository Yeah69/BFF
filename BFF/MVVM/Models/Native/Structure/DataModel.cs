using BFF.DB;
using Dapper.Contrib.Extensions;

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
        /// <param name="orm">Given ORM.</param>
        void Insert(IBffOrm orm);

        /// <summary>
        /// The model updates itself in a database provided by given ORM.
        /// </summary>
        /// <param name="orm">Given ORM.</param>
        void Update(IBffOrm orm);

        /// <summary>
        /// The model removes itself from a database provided by given ORM.
        /// </summary>
        /// <param name="orm">Given ORM.</param>
        void Delete(IBffOrm orm);
    }

    /// <summary>
    /// Base class for all model classes, which get OR-mapped
    /// </summary>
    public abstract class DataModel<T> : ObservableObject, IDataModel where T : class, IDataModel
    {
        private long _id = -1L;

        private readonly IRepository<T> _repository;

        /// <summary>
        /// Identification number for the database
        /// </summary>
        [Key]
        public long Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        protected DataModel(IRepository<T> repository, long id = -1L)
        {
            _repository = repository;
            if (Id == -1L || id > 0L) Id = id;
        }

        /// <summary>
        /// The model inserts itself to a database provided by given ORM.
        /// </summary>
        /// <param name="orm">Given ORM.</param>
        public virtual void Insert(IBffOrm orm)
        {
            _repository.Add(this as T);
        }

        /// <summary>
        /// The model updates itself in a database provided by given ORM.
        /// </summary>
        /// <param name="orm">Given ORM.</param>
        public virtual void Update(IBffOrm orm)
        {
            _repository.Update(this as T);
        }

        /// <summary>
        /// The model removes itself from a database provided by given ORM.
        /// </summary>
        /// <param name="orm">Given ORM.</param>
        public virtual void Delete(IBffOrm orm)
        {
            _repository.Delete(this as T);
        }
    }
}
