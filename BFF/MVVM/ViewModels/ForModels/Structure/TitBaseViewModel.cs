using System;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

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
        private readonly ITitBase _titBase;

        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        public virtual DateTime Date
        {
            get => _titBase.Date;
            set
            {
                if(_titBase.Date == value) return;
                _titBase.Date = value;
                OnUpdate();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public virtual bool Cleared
        {
            get => _titBase.Cleared;
            set
            {
                if(_titBase.Cleared == value) return;
                _titBase.Cleared = value;
                OnUpdate();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a TitBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="titBase">The model.</param>
        protected TitBaseViewModel(IBffOrm orm, ITitBase titBase) : base(orm, titBase)
        {
            _titBase = titBase;
        }

        #region Account Editing

        /// <summary>
        /// All currently available Accounts.
        /// </summary>
        public ObservableCollection<IAccountViewModel> AllAccounts => CommonPropertyProvider.AllAccountViewModels;

        #endregion
    }
}
