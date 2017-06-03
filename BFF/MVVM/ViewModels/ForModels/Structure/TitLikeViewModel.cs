using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITitLikeViewModel : IDataModelViewModel
    {
        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        string Memo { get; set; }

        /// <summary>
        /// The amount of money of the exchange of the TIT.
        /// </summary>
        long Sum { get; set; }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        ICommand DeleteCommand { get; }
    }

    /// <summary>
    /// Base class for all ViewModel classes of Models related to TITs (Transaction, Income, Transfer).
    /// This includes also the Parent and SubElement models.
    /// </summary>
    public abstract class TitLikeViewModel : DataModelViewModel, ITitLikeViewModel
    {
        private readonly ITitLike _titLike;

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public virtual string Memo
        {
            get => _titLike.Memo;
            set
            {
                if(_titLike.Memo == value) return;
                _titLike.Memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money of the exchange of the TIT.
        /// </summary>
        public abstract long Sum { get; set; } //todo see Memo

        /// <summary>
        /// Initializes a TitLikeViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="titLike">The model.</param>
        protected TitLikeViewModel(IBffOrm orm, ITitLike titLike) : base(orm, titLike)
        {
            _titLike = titLike;
        }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        public virtual ICommand DeleteCommand => new RelayCommand(obj => Delete());
    }
}
