using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITitBaseViewModel : ITitLikeViewModel
    {
        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        IReactiveProperty<DateTime> Date { get; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        IReactiveProperty<bool> Cleared { get; }
    }

    /// <summary>
    /// Base class for all ViewModels of Models of TITs excluding the SubElements.
    /// From this point in the documentation of the ViewModel hierarchy TIT is referring to all TIT-like Elements except SubElements.
    /// </summary>
    public abstract class TitBaseViewModel : TitLikeViewModel, ITitBaseViewModel
    {
        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        public virtual IReactiveProperty<DateTime> Date { get; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public virtual IReactiveProperty<bool> Cleared { get; }

        /// <summary>
        /// Initializes a TitBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="titBase">The model.</param>
        protected TitBaseViewModel(IBffOrm orm, ITitBase titBase) : base(orm, titBase)
        {
            Date = titBase.ToReactivePropertyAsSynchronized(tb => tb.Date);
            Cleared = titBase.ToReactivePropertyAsSynchronized(tb => tb.Cleared);
        }
    }
}
