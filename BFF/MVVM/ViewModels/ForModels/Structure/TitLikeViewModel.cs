using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITitLikeViewModel : IDataModelViewModel
    {
        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        IReactiveProperty<string> Memo { get; }

        /// <summary>
        /// The amount of money of the exchange of the TIT.
        /// </summary>
        IReactiveProperty<long> Sum { get; }

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

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public virtual IReactiveProperty<string> Memo { get; }

        /// <summary>
        /// The amount of money of the exchange of the TIT.
        /// </summary>
        public abstract IReactiveProperty<long> Sum { get; } //todo see Memo

        /// <summary>
        /// Initializes a TitLikeViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="titLike">The model.</param>
        protected TitLikeViewModel(IBffOrm orm, ITitLike titLike) : base(orm, titLike)
        {
            Memo = titLike.ToReactivePropertyAsSynchronized(tl => tl.Memo);
        }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        public virtual ICommand DeleteCommand => new RelayCommand(obj => Delete());
    }
}
