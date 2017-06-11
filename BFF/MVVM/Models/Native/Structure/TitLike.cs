using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITitLike : IDataModel
    {
        /// <summary>
        /// A note to hint on the reasons of creating this Tit
        /// </summary>
        string Memo { get; set; }
    }

    /// <summary>
    /// Base class for all classes which can be shown in the TitDataGrid (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TitLike<T> : DataModel<T>, ITitLike where T : class, ITitLike
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
        /// Initializes the TitLike-parts of the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected TitLike(IRepository<T> repository, long id = -1L, string memo = null) : base(repository, id)
        {
            _memo = memo ?? _memo;
        }

    }
}
