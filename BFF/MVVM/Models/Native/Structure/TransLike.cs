using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransLike : IDataModel
    {
        /// <summary>
        /// A note to hint on the reasons of creating this Tit
        /// </summary>
        string Memo { get; set; }
    }

    /// <summary>
    /// Base class for all classes which can be shown in the TitDataGrid (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TransLike<T> : DataModel<T>, ITransLike where T : class, ITransLike
    {
        private string _memo;

        /// <summary>
        /// A note to hint on the reasons of creating this Tit
        /// </summary>
        public string Memo
        {
            get => _memo;
            set
            {
                if(_memo == value) return;
                _memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TransLike-parts of the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected TransLike(IRepository<T> repository, long id, string memo) : base(repository, id)
        {
            _memo = memo ?? _memo;
        }

    }
}
