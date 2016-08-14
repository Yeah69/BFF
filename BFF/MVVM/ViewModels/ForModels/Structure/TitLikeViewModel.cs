using System.Windows.Input;
using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    /// <summary>
    /// Base class for all ViewModel classes of Models related to TITs (Transaction, Income, Transfer).
    /// This includes also the Parent and SubElement models.
    /// </summary>
    public abstract class TitLikeViewModel : DataModelViewModel
    {
        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public abstract string Memo { get; set; }

        /// <summary>
        /// The amount of money of the exchangement of the TIT.
        /// </summary>
        public abstract long Sum { get; set; }

        /// <summary>
        /// Initializes a TitLikeViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        protected TitLikeViewModel(IBffOrm orm) : base(orm) { }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        public virtual ICommand DeleteCommand => new RelayCommand(obj => Delete());
    }
}
