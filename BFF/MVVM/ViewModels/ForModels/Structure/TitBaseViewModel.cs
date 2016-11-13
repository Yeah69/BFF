using System;
using System.Collections.ObjectModel;
using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITitBaseViewModel : ITitLikeViewModel
    {
        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        bool Cleared { get; set; }
    }

    /// <summary>
    /// Base class for all ViewModels of Models of TITs excluding the SubElements.
    /// From this point in the documentation of the ViewModel hierarchy TIT is refering to all TIT-like Elements except SubElements.
    /// </summary>
    public abstract class TitBaseViewModel : TitLikeViewModel, ITitBaseViewModel
    {
        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        public abstract DateTime Date { get; set; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public abstract bool Cleared { get; set; }

        /// <summary>
        /// Initializes a TitBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        protected TitBaseViewModel(IBffOrm orm) : base(orm) { }

        #region Account Editing

        /// <summary>
        /// All currently available Accounts.
        /// </summary>
        public ObservableCollection<IAccountViewModel> AllAccounts => Orm?.CommonPropertyProvider.AllAccountViewModels;

        #endregion
    }
}
