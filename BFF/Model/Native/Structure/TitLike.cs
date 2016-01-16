namespace BFF.Model.Native.Structure
{
    /// <summary>
    /// Base class for all classes which can be shown in the TitDataGrid
    /// </summary>
    public abstract class TitLike : DataModelBase
    {
        private string _memo;

        /// <summary>
        /// A note to hint on the reasons of creating this Tit
        /// </summary>
        public string Memo
        {
            get { return _memo; }
            set
            {
                _memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TitLike-parts of the object
        /// </summary>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected TitLike(string memo = null)
        {
            ConstrDbLock = true;
            _memo = memo ?? _memo;
            ConstrDbLock = false;
        }
    }
}
